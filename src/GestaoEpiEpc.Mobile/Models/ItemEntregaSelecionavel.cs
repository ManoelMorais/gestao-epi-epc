using CommunityToolkit.Mvvm.ComponentModel;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Mobile.Models;

/// <summary>Um item elegível para o colaborador, com o estado de seleção e quantidade que o facilitador está registrando.</summary>
public partial class ItemEntregaSelecionavel(ItemEpiEpc item) : ObservableObject
{
    public ItemEpiEpc Item { get; } = item;

    [ObservableProperty]
    private bool selecionado;

    [ObservableProperty]
    private int quantidade = 1;
}
