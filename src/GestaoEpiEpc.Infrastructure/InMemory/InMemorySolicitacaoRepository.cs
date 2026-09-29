using System.Globalization;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemorySolicitacaoRepository(InMemoryDataStore store) : ISolicitacaoRepository
{
    public Task<Solicitacao?> ObterPorIdAsync(Guid id)
    {
        var solicitacao = store.Solicitacoes.FirstOrDefault(s => s.Id == id);
        if (solicitacao is not null) store.ResolverNavegacoes(solicitacao);
        return Task.FromResult(solicitacao);
    }

    public Task<IReadOnlyList<Solicitacao>> ListarAsync() => ConsultarAsync(new FiltroSolicitacoes());

    public Task<IReadOnlyList<Solicitacao>> ConsultarAsync(FiltroSolicitacoes filtro)
    {
        foreach (var s in store.Solicitacoes) store.ResolverNavegacoes(s);
        IEnumerable<Solicitacao> consulta = store.Solicitacoes;

        if (filtro.Status is { Count: > 0 } status)
            consulta = consulta.Where(s => status.Contains(s.Status));

        if (filtro.ColaboradorId is { } colaboradorId)
            consulta = consulta.Where(s => s.ColaboradorId == colaboradorId);

        if (filtro.UnidadeId is { } unidadeId)
            consulta = consulta.Where(s => s.Colaborador?.UnidadeId == unidadeId);

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var termo = filtro.Termo.Trim();
            consulta = consulta.Where(s =>
                Contem(s.Protocolo, termo) || Contem(s.Colaborador?.Nome, termo) ||
                Contem(s.Colaborador?.Drt, termo) || Contem(s.Item?.Nome, termo));
        }

        return Task.FromResult<IReadOnlyList<Solicitacao>>(consulta.OrderByDescending(s => s.CriadaEm).ToList());
    }

    public Task AdicionarEventoAsync(EventoSolicitacao evento)
    {
        var solicitacao = store.Solicitacoes.First(s => s.Id == evento.SolicitacaoId);
        solicitacao.Historico.Add(evento);
        return Task.CompletedTask;
    }

    public Task<int> ObterUltimoSequencialProtocoloAsync() =>
        Task.FromResult(store.Solicitacoes.Select(s => SequencialDoProtocolo(s.Protocolo)).DefaultIfEmpty(0).Max());

    public Task AdicionarAsync(Solicitacao entidade)
    {
        store.Solicitacoes.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Solicitacao entidade)
    {
        var index = store.Solicitacoes.FindIndex(s => s.Id == entidade.Id);
        if (index >= 0) store.Solicitacoes[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Solicitacoes.RemoveAll(s => s.Id == id);
        return Task.CompletedTask;
    }

    /// <summary>"SOL-2026-02401" → 2401.</summary>
    internal static int SequencialDoProtocolo(string protocolo) =>
        int.TryParse(protocolo[(protocolo.LastIndexOf('-') + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0;

    private static bool Contem(string? texto, string termo) =>
        texto is not null && CultureInfo.InvariantCulture.CompareInfo.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
}
