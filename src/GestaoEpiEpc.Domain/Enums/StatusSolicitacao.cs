namespace GestaoEpiEpc.Domain.Enums;

/// <summary>
/// Ciclo de vida de uma solicitação de troca feita pelo colaborador no app:
/// Pendente (enviada) → EmAnalise (SST/almoxarifado avaliando) → Aprovada (liberada para retirada)
/// → Entregue (vira uma <c>Entrega</c>). Pode ser Recusada durante a análise.
/// </summary>
public enum StatusSolicitacao
{
    Pendente,
    EmAnalise,
    Aprovada,
    Entregue,
    Recusada
}
