using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryItemEpiEpcRepository(InMemoryDataStore store) : IItemEpiEpcRepository
{
    public Task<ItemEpiEpc?> ObterPorIdAsync(Guid id)
    {
        var item = store.Itens.FirstOrDefault(i => i.Id == id);
        ResolverCategoria(item);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<ItemEpiEpc>> ListarAsync()
    {
        foreach (var item in store.Itens) ResolverCategoria(item);
        return Task.FromResult<IReadOnlyList<ItemEpiEpc>>(store.Itens.OrderBy(i => i.Nome).ToList());
    }

    public Task<IReadOnlyList<ItemEpiEpc>> BuscarAsync(string termo)
    {
        termo = termo.Trim();
        var resultado = store.Itens
            .Where(i => i.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) || i.Codigo.Contains(termo, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Nome)
            .ToList();

        foreach (var item in resultado) ResolverCategoria(item);
        return Task.FromResult<IReadOnlyList<ItemEpiEpc>>(resultado);
    }

    public Task<IReadOnlyList<ItemEpiEpc>> ListarPorIdsAsync(IEnumerable<Guid> ids)
    {
        var conjunto = ids.ToHashSet();
        var resultado = store.Itens.Where(i => conjunto.Contains(i.Id)).OrderBy(i => i.Nome).ToList();
        foreach (var item in resultado) ResolverCategoria(item);
        return Task.FromResult<IReadOnlyList<ItemEpiEpc>>(resultado);
    }

    public Task AdicionarAsync(ItemEpiEpc entidade)
    {
        store.Itens.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(ItemEpiEpc entidade)
    {
        var index = store.Itens.FindIndex(i => i.Id == entidade.Id);
        if (index >= 0) store.Itens[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Itens.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    private void ResolverCategoria(ItemEpiEpc? item)
    {
        if (item is not null && item.Categoria is null)
            item.Categoria = store.CategoriasItem.FirstOrDefault(c => c.Id == item.CategoriaId);
    }
}
