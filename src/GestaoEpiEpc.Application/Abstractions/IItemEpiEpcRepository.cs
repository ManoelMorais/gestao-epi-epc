using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface IItemEpiEpcRepository : IRepository<ItemEpiEpc>
{
    Task<IReadOnlyList<ItemEpiEpc>> BuscarAsync(string termo);
    Task<IReadOnlyList<ItemEpiEpc>> ListarPorIdsAsync(IEnumerable<Guid> ids);
}
