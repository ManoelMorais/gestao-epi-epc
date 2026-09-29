using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class CategoriaItem : EntidadeBase
{
    /// <summary>Identificador estável usado pelo app mobile (ex.: "cat-pes" escolhe a grade de numeração 36–44).</summary>
    public string Codigo { get; set; } = string.Empty;
    public required string Nome { get; set; }
    public required TipoItem Tipo { get; set; }
}
