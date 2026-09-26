using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

public class EntregaItem : EntidadeBase
{
    public required Guid EntregaId { get; set; }
    public Entrega? Entrega { get; set; }

    public required Guid ItemId { get; set; }
    public ItemEpiEpc? Item { get; set; }

    public required int Quantidade { get; set; }
    public string? Tamanho { get; set; }
    public string? NumeroSerie { get; set; }
    public string? FotoUrl { get; set; }
    public DateOnly? ValidadeCalculada { get; set; }
}
