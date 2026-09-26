using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryCargoItemPermitidoRepository(InMemoryDataStore store) : ICargoItemPermitidoRepository
{
    public Task<CargoItemPermitido?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.CargoItemPermitidos.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<CargoItemPermitido>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<CargoItemPermitido>>(store.CargoItemPermitidos.ToList());

    public Task<IReadOnlyList<CargoItemPermitido>> ListarPorCargoAsync(Guid cargoId) =>
        Task.FromResult<IReadOnlyList<CargoItemPermitido>>(
            store.CargoItemPermitidos.Where(p => p.CargoId == cargoId).ToList());

    public Task RemoverPorCargoAsync(Guid cargoId)
    {
        store.CargoItemPermitidos.RemoveAll(p => p.CargoId == cargoId);
        return Task.CompletedTask;
    }

    public Task AdicionarAsync(CargoItemPermitido entidade)
    {
        store.CargoItemPermitidos.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(CargoItemPermitido entidade)
    {
        var index = store.CargoItemPermitidos.FindIndex(p => p.Id == entidade.Id);
        if (index >= 0) store.CargoItemPermitidos[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.CargoItemPermitidos.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
