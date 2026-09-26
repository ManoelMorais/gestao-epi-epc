using GestaoEpiEpc.Domain.Enums;
using MaterialDesignThemes.Wpf;

namespace GestaoEpiEpc.Desktop.Models;

public class NavItem
{
    public required string Titulo { get; init; }
    public required PackIconKind Icone { get; init; }
    public required Type TipoViewModel { get; init; }
    public required PerfilUsuario[] PerfisPermitidos { get; init; }
}
