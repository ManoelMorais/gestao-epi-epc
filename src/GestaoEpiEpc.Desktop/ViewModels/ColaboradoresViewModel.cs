using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class ColaboradoresViewModel(IColaboradorService colaboradorServico) : ObservableObject, IInicializavel
{
    [ObservableProperty]
    private string termoBusca = string.Empty;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty]
    private Colaborador? colaboradorSelecionado;

    [ObservableProperty]
    private bool mostrarDetalhes;

    public ObservableCollection<Colaborador> Colaboradores { get; } = new();

    public async Task InicializarAsync() => await BuscarAsync();

    public void AbrirDetalhes(Colaborador colaborador)
    {
        ColaboradorSelecionado = colaborador;
        MostrarDetalhes = true;
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
