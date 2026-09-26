using System.Globalization;
using System.Windows.Data;

namespace GestaoEpiEpc.Desktop.Converters;

public class BoolParaSimNaoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "Sim" : "Não";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
