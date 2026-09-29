using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface ISolicitacaoRepository : IRepository<Solicitacao>
{
    Task<IReadOnlyList<Solicitacao>> ConsultarAsync(FiltroSolicitacoes filtro);
    Task AdicionarEventoAsync(EventoSolicitacao evento);
    /// <summary>Maior número sequencial já emitido (o "02401" de SOL-2026-02401), ou 0 se não houver.</summary>
    Task<int> ObterUltimoSequencialProtocoloAsync();
}
