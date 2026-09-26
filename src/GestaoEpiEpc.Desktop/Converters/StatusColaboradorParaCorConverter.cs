using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class StatusColaboradorParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Ativo = new(Color.FromRgb(0x1E, 0x82, 0x4C));
    private static readonly SolidColorBrush Inativo = new(Color.FromRgb(0x8B, 0x93, 0xA3));
    private static readonly SolidColorBrush Afastado = new(Color.FromRgb(0xE8, 0x5D, 0x19));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StatusColaborador.Ativo => Ativo,
        StatusColaborador.Afastado => Afastado,
        _ => Inativo
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
