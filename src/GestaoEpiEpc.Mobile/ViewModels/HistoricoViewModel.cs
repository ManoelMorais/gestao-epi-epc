using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Mobile.Common;

namespace GestaoEpiEpc.Mobile.ViewModels;

public partial class HistoricoViewModel(IEntregaService entregaServico, SessaoAtual sessao) : ObservableObject
{
    public ObservableCollection<Entrega> Entregas { get; } = new();

    [ObservableProperty] private bool carregando;

    public async Task CarregarAsync()
    {
        if (sessao.FacilitadorAtual is null) return;

        Carregando = true;
        try
        {
            var filtro = new FiltroEntregas { FacilitadorId = sessao.FacilitadorAtual.Id };
            var resultado = await entregaServico.ConsultarAsync(filtro);

            Entregas.Clear();
            foreach (var entrega in resultado.OrderByDescending(e => e.DataHora))
                Entregas.Add(entrega);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task AtualizarAsync() => await CarregarAsync();
}
