namespace GestaoEpiEpc.Domain.Enums;

/// <summary>
/// Facilitador usa apenas o app mobile (registro). Os demais perfis usam o desktop (acompanhamento/gestão).
/// </summary>
public enum PerfilUsuario
{
    Facilitador,
    Gestao,
    SegurancaTrabalho,
    Rh,
    Administrador
}
