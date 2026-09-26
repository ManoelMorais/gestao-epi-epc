using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class AuditoriaViewModel(IAuditoriaService auditoriaServico) : ObservableObject, IInicializavel
{
    public ObservableCollection<LogAuditoria> Logs { get; } = new();

    public static readonly string[] EntidadesConhecidas =
        ["Entrega", "CargoItemPermitido", "ItemEpiEpc", "Usuario"];

    [ObservableProperty] private DateTime? dataInicio;
    [ObservableProperty] private DateTime? dataFim;
    [ObservableProperty] private string? entidadeFiltro;
    [ObservableProperty] private LogAuditoria? logSelecionado;
    [ObservableProperty] private bool mostrarDetalhes;
    [ObservableProperty] private bool carregando;

    public async Task InicializarAsync() => await ConsultarAsync();

    public void AbrirDetalhes(LogAuditoria log)
    {
        LogSelecionado = log;
        MostrarDetalhes = true;
    }

    [RelayCommand]
    private void FecharDetalhes() => MostrarDetalhes = false;

    [RelayCommand]
    private async Task LimparFiltrosAsync()
    {
        DataInicio = null;
        DataFim = null;
        EntidadeFiltro = null;
        await ConsultarAsync();
    }

    [RelayCommand]
    private async Task ConsultarAsync()
    {
        Carregando = true;
        try
        {
            var resultado = await auditoriaServico.ConsultarAsync(
                usuarioId: null,
                entidade: string.IsNullOrWhiteSpace(EntidadeFiltro) ? null : EntidadeFiltro,
                de: DataInicio is { } di ? DateOnly.FromDateTime(di) : null,
                ate: DataFim is { } df ? DateOnly.FromDateTime(df) : null);

            Logs.Clear();
            foreach (var log in resultado)
                Logs.Add(log);
        }
        finally
        {
            Carregando = false;
        }
    }
}
