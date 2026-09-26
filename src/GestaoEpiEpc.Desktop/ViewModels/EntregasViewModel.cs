using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class EntregasViewModel(
    IEntregaService entregaServico,
    IColaboradorService colaboradorServico,
    SessaoAtual sessao) : ObservableObject, IInicializavel
{
    public ObservableCollection<Entrega> Resultados { get; } = new();
    public ObservableCollection<Colaborador> ColaboradoresDisponiveis { get; } = new();
    public IReadOnlyList<TipoMovimentacao?> TiposDisponiveis { get; } =
        new TipoMovimentacao?[] { null }.Concat(Enum.GetValues<TipoMovimentacao>().Cast<TipoMovimentacao?>()).ToList();

    [ObservableProperty] private DateTime? dataInicio;
    [ObservableProperty] private DateTime? dataFim;
    [ObservableProperty] private Colaborador? colaboradorFiltro;
    [ObservableProperty] private TipoMovimentacao? tipoMovimentacaoFiltro;
    [ObservableProperty] private bool carregando;
    [ObservableProperty] private Entrega? entregaSelecionada;
    [ObservableProperty] private bool mostrarDetalhes;

    public bool PodeEstornar => sessao.UsuarioAtual?.Perfil == PerfilUsuario.Administrador;

    public void AbrirDetalhes(Entrega entrega)
    {
        EntregaSelecionada = entrega;
        MostrarDetalhes = true;
    }

    [RelayCommand]
    private void FecharDetalhes() => MostrarDetalhes = false;

    public async Task InicializarAsync()
    {
        ColaboradoresDisponiveis.Clear();
        foreach (var colaborador in await colaboradorServico.ListarAsync())
            ColaboradoresDisponiveis.Add(colaborador);

        await ConsultarAsync();
    }

    [RelayCommand]
    private async Task LimparFiltrosAsync()
    {
        DataInicio = null;
        DataFim = null;
        ColaboradorFiltro = null;
        TipoMovimentacaoFiltro = null;
        await ConsultarAsync();
    }

    [RelayCommand]
    private async Task ConsultarAsync()
    {
        Carregando = true;
        try
        {
            var filtro = new FiltroEntregas
            {
                DataInicio = DataInicio is { } di ? DateOnly.FromDateTime(di) : null,
                DataFim = DataFim is { } df ? DateOnly.FromDateTime(df) : null,
                ColaboradorId = ColaboradorFiltro?.Id,
                TipoMovimentacao = TipoMovimentacaoFiltro
            };

            var resultado = await entregaServico.ConsultarAsync(filtro);
            Resultados.Clear();
            foreach (var entrega in resultado)
                Resultados.Add(entrega);
        }
        finally
        {
            Carregando = false;
        }
    }

    public async Task EstornarAsync(Entrega entrega, string justificativa)
    {
        if (sessao.UsuarioAtual is null) return;

        await entregaServico.EstornarAsync(entrega.Id, sessao.UsuarioAtual.Id, justificativa);
        MostrarDetalhes = false;
        await ConsultarAsync();
    }
}
