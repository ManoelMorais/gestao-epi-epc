using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class Colaborador : EntidadeBase
{
    public required string Matricula { get; set; }
    public required string Nome { get; set; }
    public required Guid CargoId { get; set; }
    public Cargo? Cargo { get; set; }
    public required string Area { get; set; }
    public required Guid UnidadeId { get; set; }
    public Unidade? Unidade { get; set; }
    public StatusColaborador Status { get; set; } = StatusColaborador.Ativo;
}
