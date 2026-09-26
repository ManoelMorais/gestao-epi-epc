using System.Globalization;
using System.Windows.Data;

namespace GestaoEpiEpc.Desktop.Converters;

/// <summary>Extrai as iniciais de um nome completo (ex. "Roberto Carlos Nascimento" -> "RN") para avatares.</summary>
public class IniciaisConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string nome || string.IsNullOrWhiteSpace(nome)) return "?";

        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..1].ToUpperInvariant(),
            _ => $"{partes[0][..1]}{partes[^1][..1]}".ToUpperInvariant()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
