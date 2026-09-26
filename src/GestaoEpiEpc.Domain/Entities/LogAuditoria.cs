using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Domain.Entities;

public class LogAuditoria : EntidadeBase
{
    public required Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public required string Entidade { get; set; }
    public required Guid EntidadeId { get; set; }
    public required string Acao { get; set; }
    public string? DadosAntes { get; set; }
    public string? DadosDepois { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
}
