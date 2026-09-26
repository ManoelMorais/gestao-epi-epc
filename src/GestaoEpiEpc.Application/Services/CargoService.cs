using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class CargoService(ICargoRepository cargosRepositorio) : ICargoService
{
    public Task<IReadOnlyList<Cargo>> ListarAsync() => cargosRepositorio.ListarAsync();

    public async Task<Cargo> SalvarAsync(Cargo cargo)
    {
        if (await cargosRepositorio.ObterPorIdAsync(cargo.Id) is null)
            await cargosRepositorio.AdicionarAsync(cargo);
        else
            await cargosRepositorio.AtualizarAsync(cargo);

        return cargo;
    }
}
