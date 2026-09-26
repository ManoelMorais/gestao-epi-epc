using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryUnidadeRepository(InMemoryDataStore store) : IUnidadeRepository
{
    public Task<Unidade?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.Unidades.FirstOrDefault(u => u.Id == id));

    public Task<IReadOnlyList<Unidade>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<Unidade>>(store.Unidades.OrderBy(u => u.Nome).ToList());

    public Task AdicionarAsync(Unidade entidade)
    {
        store.Unidades.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Unidade entidade)
    {
        var index = store.Unidades.FindIndex(u => u.Id == entidade.Id);
        if (index >= 0) store.Unidades[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Unidades.RemoveAll(u => u.Id == id);
        return Task.CompletedTask;
    }
}
