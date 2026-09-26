using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.ViewModels;

/// <summary>
/// Tela onde o Administrador define, por cargo, quais EPI/EPC podem ser entregues (RF26) —
/// é a regra que o registro de entrega no mobile vai consultar antes de liberar um item (RF27).
/// </summary>
public partial class CargosElegibilidadeViewModel(
    ICargoService cargoServico,
    ICatalogoService catalogoServico,
    IElegibilidadeService elegibilidadeServico,
    SessaoAtual sessao) : ObservableObject, IInicializavel
{
    public ObservableCollection<Cargo> Cargos { get; } = new();
    public ObservableCollection<ItemSelecionavel> ItensDoCatalogo { get; } = new();

    [ObservableProperty] private Cargo? cargoSelecionado;
    [ObservableProperty] private string novoCargoNome = string.Empty;
    [ObservableProperty] private string? mensagem;

    public async Task InicializarAsync()
    {
        await CarregarCargosAsync();
        CargoSelecionado = Cargos.FirstOrDefault();
    }

    private async Task CarregarCargosAsync()
    {
        var cargoSelecionadoId = CargoSelecionado?.Id;
        Cargos.Clear();
        foreach (var cargo in await cargoServico.ListarAsync())
            Cargos.Add(cargo);

        if (cargoSelecionadoId is { } id)
            CargoSelecionado = Cargos.FirstOrDefault(c => c.Id == id);
    }

    [RelayCommand]
    private async Task CriarCargoAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoCargoNome)) return;

        var cargo = await cargoServico.SalvarAsync(new Cargo { Nome = NovoCargoNome.Trim() });
        NovoCargoNome = string.Empty;
        await CarregarCargosAsync();
        CargoSelecionado = Cargos.FirstOrDefault(c => c.Id == cargo.Id);
    }

    partial void OnCargoSelecionadoChanged(Cargo? value) => _ = CarregarMatrizAsync(value);

    private async Task CarregarMatrizAsync(Cargo? cargo)
    {
        var todosOsItens = await catalogoServico.ListarItensAsync();
        var elegiveis = cargo is null
            ? Array.Empty<ItemEpiEpc>()
            : await elegibilidadeServico.ObterItensElegiveisPorCargoAsync(cargo.Id);

        var idsElegiveis = elegiveis.Select(i => i.Id).ToHashSet();

        ItensDoCatalogo.Clear();
        foreach (var item in todosOsItens.Where(i => i.Ativo))
            ItensDoCatalogo.Add(new ItemSelecionavel(item) { Selecionado = idsElegiveis.Contains(item.Id) });

        Mensagem = null;
    }

    [RelayCommand]
    private async Task SalvarPermissoesAsync()
    {
        if (CargoSelecionado is null || sessao.UsuarioAtual is null) return;

        var itensSelecionados = ItensDoCatalogo.Where(i => i.Selecionado).Select(i => i.Item.Id).ToList();
        await elegibilidadeServico.DefinirPermissoesAsync(CargoSelecionado.Id, itensSelecionados, sessao.UsuarioAtual.Id);

        Mensagem = $"Perfil de elegibilidade do cargo \"{CargoSelecionado.Nome}\" atualizado: {itensSelecionados.Count} item(ns) permitido(s).";
    }
}
