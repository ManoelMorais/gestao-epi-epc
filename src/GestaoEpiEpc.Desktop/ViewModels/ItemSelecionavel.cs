using CommunityToolkit.Mvvm.ComponentModel;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.ViewModels;

/// <summary>Envolve um item do catálogo com um estado de seleção, para a matriz de elegibilidade por cargo.</summary>
public partial class ItemSelecionavel(ItemEpiEpc item) : ObservableObject
{
    public ItemEpiEpc Item { get; } = item;

    [ObservableProperty]
    private bool selecionado;
}
