using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.ViewModels;

/// <summary>Um dos filtros rápidos no topo da tela (mesmos agrupamentos do app mobile).</summary>
public partial class GrupoSolicitacao(string titulo, params StatusSolicitacao[] status) : ObservableObject
{
    public string Titulo { get; } = titulo;
    public StatusSolicitacao[] Status { get; } = status;

    [ObservableProperty] private int quantidade;
    [ObservableProperty] private bool selecionado;
}

/// <summary>
/// Fila de análise das solicitações de troca que os colaboradores fazem no app mobile:
/// SST/gestão analisa, aprova ou recusa, e registra a retirada — que gera a Entrega.
/// </summary>
public partial class SolicitacoesViewModel(
    ISolicitacaoService solicitacaoServico,
    IUnidadeRepository unidadesRepositorio,
    SessaoAtual sessao) : ObservableObject, IInicializavel
{
    public ObservableCollection<GrupoSolicitacao> Grupos { get; } =
    [
        new("Em andamento", StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise) { Selecionado = true },
        new("Aguardando retirada", StatusSolicitacao.Aprovada),
        new("Entregues", StatusSolicitacao.Entregue),
        new("Recusadas", StatusSolicitacao.Recusada),
        new("Todas"),
    ];

    public ObservableCollection<Solicitacao> Solicitacoes { get; } = new();
    public ObservableCollection<Unidade> Unidades { get; } = new();

    /// <summary>Opção "todas" do combo (o ComboBox do WPF não seleciona um item nulo).</summary>
    private static readonly Unidade TodasAsUnidades = new() { Id = Guid.Empty, Nome = "Todas as unidades", Sigla = "", Cidade = "" };

    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private Unidade? unidadeFiltro;
    [ObservableProperty] private bool carregando;
    [ObservableProperty] private bool processando;
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private bool mensagemEhErro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemSelecao), nameof(PodeIniciarAnalise), nameof(PodeAprovarOuRecusar), nameof(PodeRegistrarEntrega), nameof(TemMaterialAntigo))]
    private Solicitacao? solicitacaoSelecionada;

    public bool TemSelecao => SolicitacaoSelecionada is not null;
    public bool PodeIniciarAnalise => SolicitacaoSelecionada?.Status == StatusSolicitacao.Pendente;
    public bool PodeAprovarOuRecusar => SolicitacaoSelecionada?.Status is StatusSolicitacao.Pendente or StatusSolicitacao.EmAnalise;
    public bool PodeRegistrarEntrega => SolicitacaoSelecionada?.Status == StatusSolicitacao.Aprovada;
    public bool TemMaterialAntigo => SolicitacaoSelecionada?.Motivo?.SemDevolucao == false;

    private GrupoSolicitacao GrupoAtual => Grupos.FirstOrDefault(g => g.Selecionado) ?? Grupos[^1];

    public async Task InicializarAsync()
    {
        Unidades.Clear();
        Unidades.Add(TodasAsUnidades);
        foreach (var unidade in await unidadesRepositorio.ListarAsync())
            Unidades.Add(unidade);
        _inicializando = true;
        UnidadeFiltro = TodasAsUnidades;
        _inicializando = false;

        foreach (var grupo in Grupos)
            grupo.PropertyChanged += async (_, e) =>
            {
                if (e.PropertyName == nameof(GrupoSolicitacao.Selecionado) && grupo.Selecionado)
                    await CarregarAsync();
            };

        await CarregarAsync();
    }

    private bool _inicializando;

    partial void OnUnidadeFiltroChanged(Unidade? value)
    {
        if (!_inicializando) _ = CarregarAsync();
    }

    [RelayCommand]
    private Task BuscarAsync() => CarregarAsync();

    public async Task CarregarAsync(Guid? manterSelecionada = null)
    {
        Carregando = true;
        try
        {
            // Uma consulta só: as contagens de cada aba e a lista filtrada saem do mesmo resultado.
            var todas = await solicitacaoServico.ConsultarAsync(new FiltroSolicitacoes
            {
                Termo = string.IsNullOrWhiteSpace(TermoBusca) ? null : TermoBusca,
                UnidadeId = UnidadeFiltro is { Id: var id } && id != Guid.Empty ? id : null
            });

            foreach (var grupo in Grupos)
                grupo.Quantidade = grupo.Status.Length == 0 ? todas.Count : todas.Count(s => grupo.Status.Contains(s.Status));

            var grupoAtual = GrupoAtual;
            var visiveis = grupoAtual.Status.Length == 0 ? todas : todas.Where(s => grupoAtual.Status.Contains(s.Status)).ToList();

            var idSelecionado = manterSelecionada ?? SolicitacaoSelecionada?.Id;
            Solicitacoes.Clear();
            foreach (var s in visiveis)
                Solicitacoes.Add(s);

            SolicitacaoSelecionada = Solicitacoes.FirstOrDefault(s => s.Id == idSelecionado) ?? Solicitacoes.FirstOrDefault();
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private Task IniciarAnaliseAsync() =>
        ExecutarAsync(s => solicitacaoServico.IniciarAnaliseAsync(s.Id, UsuarioId), s => $"{s.Protocolo} em análise.");

    public Task AprovarAsync(string? comentario) =>
        ExecutarAsync(s => solicitacaoServico.AprovarAsync(s.Id, UsuarioId, comentario), s => $"{s.Protocolo} aprovada — o colaborador já vê no app que pode retirar o item.");

    public Task RecusarAsync(string motivo) =>
        ExecutarAsync(s => solicitacaoServico.RecusarAsync(s.Id, UsuarioId, motivo), s => $"{s.Protocolo} recusada.");

    public Task RegistrarEntregaAsync() =>
        ExecutarAsync(s => solicitacaoServico.RegistrarEntregaAsync(s.Id, UsuarioId), s => $"Entrega de {s.Protocolo} registrada e incluída no histórico do colaborador.");

    private Guid UsuarioId => sessao.UsuarioAtual?.Id ?? throw new RegraDeNegocioException("Nenhum usuário conectado.");

    private async Task ExecutarAsync(Func<Solicitacao, Task> acao, Func<Solicitacao, string> sucesso)
    {
        if (SolicitacaoSelecionada is not { } selecionada || Processando) return;

        Processando = true;
        try
        {
            await acao(selecionada);
            Mensagem = sucesso(selecionada);
            MensagemEhErro = false;
            // A solicitação pode ter mudado de aba; se saiu da lista, a seleção cai na primeira.
            await CarregarAsync(selecionada.Id);
        }
        catch (Exception ex) when (ex is RegraDeNegocioException or ItemNaoElegivelException)
        {
            Mensagem = ex.Message;
            MensagemEhErro = true;
        }
        finally
        {
            Processando = false;
        }
    }
}
