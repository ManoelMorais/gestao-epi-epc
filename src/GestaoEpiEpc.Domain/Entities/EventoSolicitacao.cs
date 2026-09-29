using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>Uma etapa da linha do tempo de uma <see cref="Solicitacao"/> (quem mudou o status, quando e por quê).</summary>
public class EventoSolicitacao : EntidadeBase
{
    public required Guid SolicitacaoId { get; set; }
    public Solicitacao? Solicitacao { get; set; }

    public required StatusSolicitacao Status { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public required string Responsavel { get; set; }
    public string? Comentario { get; set; }
}
