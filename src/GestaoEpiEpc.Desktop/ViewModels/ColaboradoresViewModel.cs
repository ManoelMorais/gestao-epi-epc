using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.ViewModels;

/// <summary>
/// Consulta de colaboradores. A ficha de cada um reúne o que está em posse (a mesma visão do app)
/// e o histórico completo de movimentações — inclusive as que não vieram de solicitações do app,
/// como entregas iniciais e devoluções — com o estorno (RF25) disponível ao Administrador.
/// </summary>
public partial class ColaboradoresViewModel(
    IColaboradorService colaboradorServico,
    IEntregaService entregaServico,
    SessaoAtual sessao) : ObservableObject, IInicializavel
{
    [ObservableProperty]
    private string termoBusca = string.Empty;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty]
    private Colaborador? colaboradorSelecionado;

    [ObservableProperty]
    private bool mostrarDetalhes;

    [ObservableProperty]
    private bool carregandoFicha;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrandoPosse))]
    private bool mostrandoHistorico;

    public bool MostrandoPosse
    {
        get => !MostrandoHistorico;
        set => MostrandoHistorico = !value;
    }

    public ObservableCollection<Colaborador> Colaboradores { get; } = new();

    /// <summary>O que está com o colaborador selecionado (mesma visão da tela inicial do app).</summary>
    public ObservableCollection<ItemEmPosse> ItensEmPosse { get; } = new();

    /// <summary>Todas as movimentações do colaborador, da mais recente para a mais antiga.</summary>
    public ObservableCollection<Entrega> Historico { get; } = new();

    public bool PodeEstornar => sessao.UsuarioAtual?.Perfil == PerfilUsuario.Administrador;

    public async Task InicializarAsync() => await BuscarAsync();

    public async void AbrirDetalhes(Colaborador colaborador)
    {
        ColaboradorSelecionado = colaborador;
        MostrandoHistorico = false;
        MostrarDetalhes = true;
        await CarregarFichaAsync(colaborador);
    }

    private async Task CarregarFichaAsync(Colaborador colaborador)
    {
        ItensEmPosse.Clear();
        Historico.Clear();
        CarregandoFicha = true;
        try
        {
            foreach (var item in await colaboradorServico.ObterItensEmPosseAsync(colaborador.Id))
                ItensEmPosse.Add(item);

            foreach (var entrega in await entregaServico.ObterHistoricoPorColaboradorAsync(colaborador.Id))
                Historico.Add(entrega);
        }
        finally
        {
            CarregandoFicha = false;
        }
    }

    public async Task EstornarAsync(Entrega entrega, string justificativa)
    {
        if (sessao.UsuarioAtual is null || ColaboradorSelecionado is null) return;

        await entregaServico.EstornarAsync(entrega.Id, sessao.UsuarioAtual.Id, justificativa);
        // O estorno muda o que está em posse: recarrega as duas abas.
        await CarregarFichaAsync(ColaboradorSelecionado);
        MostrandoHistorico = true;
    }

    [RelayCommand]
    private void FecharDetalhes() => MostrarDetalhes = false;

    [RelayCommand]
    private async Task BuscarAsync()
    {
        Carregando = true;
        try
        {
            var resultado = string.IsNullOrWhiteSpace(TermoBusca)
                ? await colaboradorServico.ListarAsync()
                : await colaboradorServico.BuscarAsync(TermoBusca);

            Colaboradores.Clear();
            foreach (var colaborador in resultado)
                Colaboradores.Add(colaborador);
        }
        finally
        {
            Carregando = false;
        }
    }
}
