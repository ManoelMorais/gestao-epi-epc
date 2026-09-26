using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public interface IAuditoriaService
{
    Task<IReadOnlyList<LogAuditoria>> ConsultarAsync(Guid? usuarioId = null, string? entidade = null, DateOnly? de = null, DateOnly? ate = null);
}
