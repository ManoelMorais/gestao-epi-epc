using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using GestaoEpiEpc.Mobile.Common;
using GestaoEpiEpc.Mobile.Models;

namespace GestaoEpiEpc.Mobile.ViewModels;

public partial class RegistrarEntregaViewModel(
    IColaboradorService colaboradorServico,
    IElegibilidadeService elegibilidadeServico,
    IEntregaService entregaServico,
    IUsuarioService usuarioServico,
    IMotivoMovimentacaoRepository motivoRepositorio,
    SessaoAtual sessao) : ObservableObject
{
    public ObservableCollection<Usuario> FacilitadoresDisponiveis { get; } = new();
    public ObservableCollection<Colaborador> ResultadosBusca { get; } = new();
    public ObservableCollection<ItemEntregaSelecionavel> ItensElegiveis { get; } = new();
    public ObservableCollection<MotivoMovimentacao> Motivos { get; } = new();

    public IReadOnlyList<TipoMovimentacao> TiposDisponiveis { get; } = Enum.GetValues<TipoMovimentacao>();

    [ObservableProperty] private Usuario? facilitadorSelecionado;
    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private Colaborador? colaboradorSelecionado;
    [ObservableProperty] private TipoMovimentacao tipoSelecionado = TipoMovimentacao.Reposicao;
    [ObservableProperty] private MotivoMovimentacao? motivoSelecionado;
    [ObservableProperty] private string? observacao;
    [ObservableProperty] private bool carregandoItens;
    [ObservableProperty] private bool registrando;
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private bool mensagemEhErro;

    public async Task InicializarAsync()
    {
        FacilitadoresDisponiveis.Clear();
        foreach (var usuario in await usuarioServico.ListarAsync())
            if (usuario.Perfil == PerfilUsuario.Facilitador && usuario.Ativo)
                FacilitadoresDisponiveis.Add(usuario);

        FacilitadorSelecionado ??= FacilitadoresDisponiveis.FirstOrDefault();
        sessao.FacilitadorAtual = FacilitadorSelecionado;

        Motivos.Clear();
        foreach (var motivo in await motivoRepositorio.ListarAsync())
            Motivos.Add(motivo);
        MotivoSelecionado ??= Motivos.FirstOrDefault();
    }

    partial void OnFacilitadorSelecionadoChanged(Usuario? value) => sessao.FacilitadorAtual = value;

    [RelayCommand]
    private async Task BuscarAsync()
    {
        ResultadosBusca.Clear();
        if (string.IsNullOrWhiteSpace(TermoBusca)) return;

        foreach (var colaborador in await colaboradorServico.BuscarAsync(TermoBusca))
            ResultadosBusca.Add(colaborador);
    }

    [RelayCommand]
    private async Task SelecionarColaboradorAsync(Colaborador? colaborador)
    {
        if (colaborador is null) return;

        ColaboradorSelecionado = colaborador;
        ResultadosBusca.Clear();
        TermoBusca = string.Empty;
        Mensagem = null;

        CarregandoItens = true;
        try
        {
            ItensElegiveis.Clear();
            foreach (var item in await elegibilidadeServico.ObterItensElegiveisPorColaboradorAsync(colaborador.Id))
                ItensElegiveis.Add(new ItemEntregaSelecionavel(item));
        }
        finally
        {
            CarregandoItens = false;
        }
    }

    [RelayCommand]
    private void TrocarColaborador()
    {
        // Não mexe em ItensElegiveis aqui: a seção que a exibe já fica oculta (IsVisible segue
        // ColaboradorSelecionado) e ela é sempre limpa/repopulada no próximo SelecionarColaboradorAsync.
        // Limpar a coleção no mesmo instante em que a visibilidade muda causava um travamento
        // (reentrância no layout do CollectionView do WinUI).
        ColaboradorSelecionado = null;
        Mensagem = null;
    }

    [RelayCommand]
    private async Task RegistrarAsync()
    {
        Mensagem = null;

        if (FacilitadorSelecionado is null)
        {
            MensagemEhErro = true;
            Mensagem = "Selecione qual facilitador está registrando.";
            return;
        }

        if (ColaboradorSelecionado is null)
        {
            MensagemEhErro = true;
            Mensagem = "Busque e selecione um colaborador.";
            return;
        }

        if (MotivoSelecionado is null)
        {
            MensagemEhErro = true;
            Mensagem = "Selecione o motivo da movimentação.";
            return;
        }

        var itensSelecionados = ItensElegiveis.Where(i => i.Selecionado).ToList();
        if (itensSelecionados.Count == 0)
        {
            MensagemEhErro = true;
            Mensagem = "Marque ao menos um item para entregar.";
            return;
        }

        Registrando = true;
        try
        {
            var input = new RegistrarEntregaInput
            {
                ColaboradorId = ColaboradorSelecionado.Id,
                FacilitadorId = FacilitadorSelecionado.Id,
                UnidadeId = FacilitadorSelecionado.UnidadeId,
                MotivoId = MotivoSelecionado.Id,
                TipoMovimentacao = TipoSelecionado,
                Observacao = string.IsNullOrWhiteSpace(Observacao) ? null : Observacao.Trim(),
                Itens = itensSelecionados
                    .Select(i => new RegistrarEntregaItemInput { ItemId = i.Item.Id, Quantidade = i.Quantidade })
                    .ToList()
            };

            await entregaServico.RegistrarAsync(input);

            MensagemEhErro = false;
            Mensagem = $"Entrega registrada para {ColaboradorSelecionado.Nome}.";
            TrocarColaborador();
            Observacao = null;
        }
        catch (ItemNaoElegivelException ex)
        {
            MensagemEhErro = true;
            Mensagem = ex.Message;
        }
        finally
        {
            Registrando = false;
        }
    }
}
