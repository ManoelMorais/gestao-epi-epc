using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>
/// Cabeçalho de uma movimentação de EPI/EPC. Nunca é apagada fisicamente:
/// um estorno cria uma nova <see cref="Entrega"/> referenciando esta em <see cref="EntregaOrigemId"/>.
/// </summary>
public class Entrega : EntidadeBase
{
    public required Guid ColaboradorId { get; set; }
    public Colaborador? Colaborador { get; set; }

    public required Guid FacilitadorId { get; set; }
    public Usuario? Facilitador { get; set; }

    public required Guid UnidadeId { get; set; }
    public Unidade? Unidade { get; set; }

    public required Guid MotivoId { get; set; }
    public MotivoMovimentacao? Motivo { get; set; }

    public Guid? EntregaOrigemId { get; set; }
    public Entrega? EntregaOrigem { get; set; }

    public DateTime DataHora { get; set; } = DateTime.Now;
    public required TipoMovimentacao TipoMovimentacao { get; set; }
    public StatusEntrega Status { get; set; } = StatusEntrega.Confirmada;
    public string? AssinaturaUrl { get; set; }
    public string? Observacao { get; set; }

    public ICollection<EntregaItem> Itens { get; set; } = new List<EntregaItem>();
}
