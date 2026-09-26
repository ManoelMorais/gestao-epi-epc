using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface IEntregaRepository : IRepository<Entrega>
{
    Task<IReadOnlyList<Entrega>> ConsultarAsync(FiltroEntregas filtro);
    Task<IReadOnlyList<Entrega>> ListarPorColaboradorAsync(Guid colaboradorId);
}
