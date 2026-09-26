using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class CatalogoService(
    ICategoriaItemRepository categoriasRepositorio,
    IItemEpiEpcRepository itensRepositorio) : ICatalogoService
{
    public Task<IReadOnlyList<CategoriaItem>> ListarCategoriasAsync() =>
        categoriasRepositorio.ListarAsync();

    public async Task<CategoriaItem> SalvarCategoriaAsync(CategoriaItem categoria)
    {
        if (await categoriasRepositorio.ObterPorIdAsync(categoria.Id) is null)
            await categoriasRepositorio.AdicionarAsync(categoria);
        else
            await categoriasRepositorio.AtualizarAsync(categoria);

        return categoria;
    }

    public Task<IReadOnlyList<ItemEpiEpc>> ListarItensAsync() =>
        itensRepositorio.ListarAsync();

    public Task<IReadOnlyList<ItemEpiEpc>> BuscarItensAsync(string termo) =>
        itensRepositorio.BuscarAsync(termo);

    public async Task<ItemEpiEpc> SalvarItemAsync(ItemEpiEpc item)
    {
        if (await itensRepositorio.ObterPorIdAsync(item.Id) is null)
            await itensRepositorio.AdicionarAsync(item);
        else
            await itensRepositorio.AtualizarAsync(item);

        return item;
    }

    public async Task InativarItemAsync(Guid itemId)
    {
        var item = await itensRepositorio.ObterPorIdAsync(itemId)
            ?? throw new InvalidOperationException("Item não encontrado.");

        item.Ativo = false;
        item.AtualizadoEm = DateTime.Now;
        await itensRepositorio.AtualizarAsync(item);
    }
}
