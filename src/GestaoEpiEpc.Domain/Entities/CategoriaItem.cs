using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class CategoriaItem : EntidadeBase
{
    public required string Nome { get; set; }
    public required TipoItem Tipo { get; set; }
}
