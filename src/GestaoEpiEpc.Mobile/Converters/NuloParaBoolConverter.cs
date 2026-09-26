using System.Globalization;

namespace GestaoEpiEpc.Mobile.Converters;

/// <summary>Converte um valor "presente" (objeto não nulo, bool true, string não vazia) em bool.
/// Passe ConverterParameter="inverso" para inverter o resultado.</summary>
public class NuloParaBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resultado = value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };

        if (parameter is string p && p.Equals("inverso", StringComparison.OrdinalIgnoreCase))
            resultado = !resultado;

        return resultado;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
