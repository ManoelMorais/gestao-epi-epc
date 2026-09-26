using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>
/// Associação N:N entre <see cref="Cargo"/> e <see cref="ItemEpiEpc"/>:
/// a existência de um registro aqui é o que torna um item elegível para um cargo.
/// </summary>
public class CargoItemPermitido : EntidadeBase
{
    public required Guid CargoId { get; set; }
    public Cargo? Cargo { get; set; }
    public required Guid ItemId { get; set; }
    public ItemEpiEpc? Item { get; set; }
}
