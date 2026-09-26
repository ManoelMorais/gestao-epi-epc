using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface ILogAuditoriaRepository : IRepository<LogAuditoria>
{
    Task RegistrarAsync(Guid usuarioId, string entidade, Guid entidadeId, string acao, string? dadosAntes = null, string? dadosDepois = null);
    Task<IReadOnlyList<LogAuditoria>> ConsultarAsync(Guid? usuarioId, string? entidade, DateOnly? de, DateOnly? ate);
}
