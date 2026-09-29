using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

/// <summary>
/// Solicitações de troca feitas pelos colaboradores no app mobile. O app cria (<see cref="CriarAsync"/>);
/// o desktop analisa, aprova ou recusa e registra a retirada, que gera a <see cref="Entrega"/>.
/// Todas as transições inválidas lançam <see cref="Exceptions.RegraDeNegocioException"/>.
/// </summary>
public interface ISolicitacaoService
{
    Task<IReadOnlyList<Solicitacao>> ConsultarAsync(FiltroSolicitacoes filtro);
    Task<Solicitacao?> ObterAsync(Guid id);

    Task<Solicitacao> CriarAsync(Guid colaboradorId, NovaSolicitacaoInput input);

    Task IniciarAnaliseAsync(Guid solicitacaoId, Guid usuarioId);
    Task AprovarAsync(Guid solicitacaoId, Guid usuarioId, string? comentario = null);
    Task RecusarAsync(Guid solicitacaoId, Guid usuarioId, string motivo);

    /// <summary>Colaborador retirou o item: registra a Entrega (validando a elegibilidade) e fecha a solicitação.</summary>
    Task<Entrega> RegistrarEntregaAsync(Guid solicitacaoId, Guid usuarioId, string? comentario = null);
}
