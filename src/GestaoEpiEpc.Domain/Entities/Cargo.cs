using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>
/// Função/posição do colaborador. Base da regra de elegibilidade: define,
/// via <see cref="CargoItemPermitido"/>, quais EPI/EPC podem ser entregues a quem exerce este cargo.
/// </summary>
public class Cargo : EntidadeBase
{
    public required string Nome { get; set; }
}
