using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class AuditoriaService(ILogAuditoriaRepository auditoriaRepositorio) : IAuditoriaService
{
    public Task<IReadOnlyList<LogAuditoria>> ConsultarAsync(Guid? usuarioId = null, string? entidade = null, DateOnly? de = null, DateOnly? ate = null) =>
        auditoriaRepositorio.ConsultarAsync(usuarioId, entidade, de, ate);
}
