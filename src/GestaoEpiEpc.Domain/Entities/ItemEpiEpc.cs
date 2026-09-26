using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

public class ItemEpiEpc : EntidadeBase
{
    public required string Codigo { get; set; }
    public required string Nome { get; set; }
    public required Guid CategoriaId { get; set; }
    public CategoriaItem? Categoria { get; set; }
    public string? NumeroCa { get; set; }
    public int? ValidadePadraoMeses { get; set; }
    public bool PossuiTamanho { get; set; }
    public bool Ativo { get; set; } = true;
}
