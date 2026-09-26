using System.Globalization;
using System.Windows.Data;
using GestaoEpiEpc.Domain.Enums;
using MaterialDesignThemes.Wpf;

namespace GestaoEpiEpc.Desktop.Converters;

public class TipoItemParaIconeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoItem.Epc ? PackIconKind.TrafficCone : PackIconKind.ShieldAccountOutline;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
