using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

/// <summary>Gestão do catálogo de EPI/EPC — categorias e itens (RF07-RF09).</summary>
public interface ICatalogoService
{
    Task<IReadOnlyList<CategoriaItem>> ListarCategoriasAsync();
    Task<CategoriaItem> SalvarCategoriaAsync(CategoriaItem categoria);

    Task<IReadOnlyList<ItemEpiEpc>> ListarItensAsync();
    Task<IReadOnlyList<ItemEpiEpc>> BuscarItensAsync(string termo);
    Task<ItemEpiEpc> SalvarItemAsync(ItemEpiEpc item);

    /// <summary>Inativa (não remove) um item, preservando o histórico de entregas já feitas (RF09).</summary>
    Task InativarItemAsync(Guid itemId);
}
