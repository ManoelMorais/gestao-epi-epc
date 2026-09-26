using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class PerfilUsuarioParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Azul = new(Color.FromRgb(0x1D, 0x4E, 0xD8));
    private static readonly SolidColorBrush Laranja = new(Color.FromRgb(0xE8, 0x5D, 0x19));
    private static readonly SolidColorBrush Verde = new(Color.FromRgb(0x1E, 0x82, 0x4C));
    private static readonly SolidColorBrush Roxo = new(Color.FromRgb(0x6D, 0x28, 0xD9));
    private static readonly SolidColorBrush Ciano = new(Color.FromRgb(0x0E, 0x74, 0x90));
    private static readonly SolidColorBrush Cinza = new(Color.FromRgb(0x6B, 0x72, 0x80));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        PerfilUsuario.Administrador => Roxo,
        PerfilUsuario.Gestao => Azul,
        PerfilUsuario.SegurancaTrabalho => Laranja,
        PerfilUsuario.Rh => Verde,
        PerfilUsuario.Facilitador => Ciano,
        _ => Cinza
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
