using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>
/// Funcionário que recebe EPI/EPC. Também é o usuário do app mobile: entra com o DRT e a senha
/// para acompanhar os itens em posse e pedir trocas (<see cref="Solicitacao"/>).
/// </summary>
public class Colaborador : EntidadeBase
{
    /// <summary>DRT — número de registro do colaborador; é o login dele no app.</summary>
    public required string Drt { get; set; }
    public required string Nome { get; set; }
    public required Guid CargoId { get; set; }
    public Cargo? Cargo { get; set; }
    public required string Area { get; set; }
    public required Guid UnidadeId { get; set; }
    public Unidade? Unidade { get; set; }
    public StatusColaborador Status { get; set; } = StatusColaborador.Ativo;
    public DateOnly Admissao { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    /// <summary>Hash PBKDF2 da senha do app (nunca a senha em texto). Nulo = ainda sem acesso ao app.</summary>
    public string? SenhaHash { get; set; }
}
