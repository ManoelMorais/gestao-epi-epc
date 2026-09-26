using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

public class Unidade : EntidadeBase
{
    public required string Nome { get; set; }
    public required string Sigla { get; set; }
}
