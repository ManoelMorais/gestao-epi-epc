using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class StatusEntregaParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Confirmada = new(Color.FromRgb(0x1E, 0x82, 0x4C));
    private static readonly SolidColorBrush Estornada = new(Color.FromRgb(0xC4, 0x32, 0x1A));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is StatusEntrega.Estornada ? Estornada : Confirmada;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
