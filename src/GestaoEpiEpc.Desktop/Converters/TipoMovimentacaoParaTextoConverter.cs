using System.Globalization;
using System.Windows.Data;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class TipoMovimentacaoParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        TipoMovimentacao.EntregaInicial => "Entrega Inicial",
        TipoMovimentacao.Reposicao => "Reposição",
        TipoMovimentacao.Troca => "Troca",
        TipoMovimentacao.Devolucao => "Devolução",
        null => "Todos",
        _ => value.ToString() ?? string.Empty
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
