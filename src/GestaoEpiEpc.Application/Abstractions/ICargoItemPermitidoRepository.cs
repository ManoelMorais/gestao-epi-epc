using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface ICargoItemPermitidoRepository : IRepository<CargoItemPermitido>
{
    Task<IReadOnlyList<CargoItemPermitido>> ListarPorCargoAsync(Guid cargoId);
    Task RemoverPorCargoAsync(Guid cargoId);
}
