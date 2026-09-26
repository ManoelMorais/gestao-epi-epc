using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryCargoRepository(InMemoryDataStore store) : ICargoRepository
{
    public Task<Cargo?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.Cargos.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Cargo>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<Cargo>>(store.Cargos.OrderBy(c => c.Nome).ToList());

    public Task AdicionarAsync(Cargo entidade)
    {
        store.Cargos.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Cargo entidade)
    {
        var index = store.Cargos.FindIndex(c => c.Id == entidade.Id);
        if (index >= 0) store.Cargos[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Cargos.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
