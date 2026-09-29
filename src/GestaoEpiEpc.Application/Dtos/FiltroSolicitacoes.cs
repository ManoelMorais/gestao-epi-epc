using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Dtos;

public class FiltroSolicitacoes
{
    /// <summary>Vazio = todos os status.</summary>
    public IReadOnlyCollection<StatusSolicitacao>? Status { get; set; }
    public Guid? ColaboradorId { get; set; }
    public Guid? UnidadeId { get; set; }
    /// <summary>Protocolo, nome/DRT do colaborador ou nome do item.</summary>
    public string? Termo { get; set; }
}
