using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public interface ICargoService
{
    Task<IReadOnlyList<Cargo>> ListarAsync();
    Task<Cargo> SalvarAsync(Cargo cargo);
}
