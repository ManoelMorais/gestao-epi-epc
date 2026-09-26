using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryCategoriaItemRepository(InMemoryDataStore store) : ICategoriaItemRepository
{
    public Task<CategoriaItem?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.CategoriasItem.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<CategoriaItem>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<CategoriaItem>>(store.CategoriasItem.OrderBy(c => c.Nome).ToList());

    public Task AdicionarAsync(CategoriaItem entidade)
    {
        store.CategoriasItem.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(CategoriaItem entidade)
    {
        var index = store.CategoriasItem.FindIndex(c => c.Id == entidade.Id);
        if (index >= 0) store.CategoriasItem[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.CategoriasItem.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
