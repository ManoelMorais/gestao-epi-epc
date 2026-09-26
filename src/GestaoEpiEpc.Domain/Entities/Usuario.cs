using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class Usuario : EntidadeBase
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required PerfilUsuario Perfil { get; set; }
    public required Guid UnidadeId { get; set; }
    public Unidade? Unidade { get; set; }
    public bool Ativo { get; set; } = true;
}
