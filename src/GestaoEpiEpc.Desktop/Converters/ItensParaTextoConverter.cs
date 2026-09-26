using System.Globalization;
using System.Windows.Data;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Converters;

public class ItensParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<EntregaItem> itens) return string.Empty;

        return string.Join("; ", itens.Select(i => $"{i.Item?.Nome ?? "?"} x{i.Quantidade}"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
