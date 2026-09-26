using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    /// <summary>Busca por nome ou matrícula (usado tanto no mobile quanto no desktop).</summary>
    Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo);
}
