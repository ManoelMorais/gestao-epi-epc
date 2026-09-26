using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryLogAuditoriaRepository(InMemoryDataStore store) : ILogAuditoriaRepository
{
    public Task<LogAuditoria?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.LogsAuditoria.FirstOrDefault(l => l.Id == id));

    public Task<IReadOnlyList<LogAuditoria>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<LogAuditoria>>(store.LogsAuditoria.OrderByDescending(l => l.DataHora).ToList());

    public Task RegistrarAsync(Guid usuarioId, string entidade, Guid entidadeId, string acao, string? dadosAntes = null, string? dadosDepois = null)
    {
        store.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            DadosAntes = dadosAntes,
            DadosDepois = dadosDepois
        });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LogAuditoria>> ConsultarAsync(Guid? usuarioId, string? entidade, DateOnly? de, DateOnly? ate)
    {
        IEnumerable<LogAuditoria> consulta = store.LogsAuditoria;

        if (usuarioId is { } uid) consulta = consulta.Where(l => l.UsuarioId == uid);
        if (!string.IsNullOrWhiteSpace(entidade)) consulta = consulta.Where(l => l.Entidade == entidade);
        if (de is { } dataInicio) consulta = consulta.Where(l => DateOnly.FromDateTime(l.DataHora) >= dataInicio);
        if (ate is { } dataFim) consulta = consulta.Where(l => DateOnly.FromDateTime(l.DataHora) <= dataFim);

        var resultado = consulta.OrderByDescending(l => l.DataHora).ToList();
        foreach (var log in resultado)
            log.Usuario ??= store.Usuarios.FirstOrDefault(u => u.Id == log.UsuarioId);

        return Task.FromResult<IReadOnlyList<LogAuditoria>>(resultado);
    }

    public Task AdicionarAsync(LogAuditoria entidade)
    {
        store.LogsAuditoria.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(LogAuditoria entidade)
    {
        var index = store.LogsAuditoria.FindIndex(l => l.Id == entidade.Id);
        if (index >= 0) store.LogsAuditoria[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.LogsAuditoria.RemoveAll(l => l.Id == id);
        return Task.CompletedTask;
    }
}
