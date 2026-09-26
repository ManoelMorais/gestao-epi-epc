using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class CatalogoViewModel(ICatalogoService catalogoServico) : ObservableObject, IInicializavel
{
    public ObservableCollection<ItemEpiEpc> Itens { get; } = new();
    public ObservableCollection<CategoriaItem> Categorias { get; } = new();
    public IReadOnlyList<TipoItem> TiposDisponiveis { get; } = Enum.GetValues<TipoItem>();

    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private ItemEpiEpc? itemSelecionado;

    // Campos do formulário de edição/criação — separados da lista para não editar "ao vivo" a linha da grade.
    [ObservableProperty] private Guid itemEmEdicaoId;
    [ObservableProperty] private string codigo = string.Empty;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private CategoriaItem? categoriaSelecionada;
    [ObservableProperty] private string? numeroCa;
    [ObservableProperty] private int? validadePadraoMeses;
    [ObservableProperty] private bool possuiTamanho;
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private bool mostrarDetalhes;
    [ObservableProperty] private bool carregando;

    public void AbrirDetalhes(ItemEpiEpc item)
    {
        ItemSelecionado = item;
        MostrarDetalhes = true;
    }

    [RelayCommand]
    private void FecharDetalhes() => MostrarDetalhes = false;

    public async Task InicializarAsync()
    {
        await CarregarCategoriasAsync();
        await BuscarAsync();
    }

    private async Task CarregarCategoriasAsync()
    {
        Categorias.Clear();
        foreach (var categoria in await catalogoServico.ListarCategoriasAsync())
            Categorias.Add(categoria);
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        Carregando = true;
        try
        {
            var resultado = string.IsNullOrWhiteSpace(TermoBusca)
                ? await catalogoServico.ListarItensAsync()
                : await catalogoServico.BuscarItensAsync(TermoBusca);

            Itens.Clear();
            foreach (var item in resultado)
                Itens.Add(item);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void NovoItem()
    {
        ItemEmEdicaoId = Guid.Empty;
        Codigo = string.Empty;
        Nome = string.Empty;
        CategoriaSelecionada = Categorias.FirstOrDefault();
        NumeroCa = null;
        ValidadePadraoMeses = null;
        PossuiTamanho = false;
        Mensagem = null;
    }

    partial void OnItemSelecionadoChanged(ItemEpiEpc? value)
    {
        if (value is null) return;

        ItemEmEdicaoId = value.Id;
        Codigo = value.Codigo;
        Nome = value.Nome;
        CategoriaSelecionada = Categorias.FirstOrDefault(c => c.Id == value.CategoriaId);
        NumeroCa = value.NumeroCa;
        ValidadePadraoMeses = value.ValidadePadraoMeses;
        PossuiTamanho = value.PossuiTamanho;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Codigo) || CategoriaSelecionada is null)
        {
            Mensagem = "Preencha código, nome e categoria antes de salvar.";
            return;
        }

        var item = new ItemEpiEpc
        {
            Id = ItemEmEdicaoId == Guid.Empty ? Guid.NewGuid() : ItemEmEdicaoId,
            Codigo = Codigo.Trim(),
            Nome = Nome.Trim(),
            CategoriaId = CategoriaSelecionada.Id,
            NumeroCa = string.IsNullOrWhiteSpace(NumeroCa) ? null : NumeroCa.Trim(),
            ValidadePadraoMeses = ValidadePadraoMeses,
            PossuiTamanho = PossuiTamanho,
            Ativo = true
        };

        await catalogoServico.SalvarItemAsync(item);
        Mensagem = "Item salvo.";
        NovoItem();
        await BuscarAsync();
    }

    [RelayCommand]
    private async Task InativarAsync(ItemEpiEpc? item)
    {
        if (item is null) return;
        await catalogoServico.InativarItemAsync(item.Id);
        MostrarDetalhes = false;
        await BuscarAsync();
    }
}
