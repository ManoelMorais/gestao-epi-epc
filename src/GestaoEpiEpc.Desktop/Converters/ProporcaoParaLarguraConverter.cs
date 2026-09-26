using System.Globalization;
using System.Windows.Data;

namespace GestaoEpiEpc.Desktop.Converters;

/// <summary>Converte uma proporção (0-1) em uma largura de pixels, para desenhar barras simples sem depender de biblioteca de gráficos.</summary>
public class ProporcaoParaLarguraConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var proporcao = value is double d ? d : 0d;
        var larguraMaxima = parameter is string texto && double.TryParse(texto, out var max) ? max : 200d;
        return Math.Max(3, proporcao * larguraMaxima);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
