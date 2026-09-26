using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GestaoEpiEpc.Desktop.Converters;

/// <summary>Mostra o painel de "nenhum resultado" apenas quando a lista não está carregando e está vazia.</summary>
public class EstadoVazioParaVisibilidadeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var carregando = values.Length > 0 && values[0] is true;
        var quantidade = values.Length > 1 && values[1] is int q ? q : 0;
        return !carregando && quantidade == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
