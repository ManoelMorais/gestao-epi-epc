namespace GestaoEpiEpc.Domain.Common;

public abstract class EntidadeBase
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CriadoEm { get; init; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
}
