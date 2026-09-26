using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class TipoMovimentacaoParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Azul = new(Color.FromRgb(0x1D, 0x4E, 0xD8));
    private static readonly SolidColorBrush Cinza = new(Color.FromRgb(0x6B, 0x72, 0x80));
    private static readonly SolidColorBrush Laranja = new(Color.FromRgb(0xE8, 0x5D, 0x19));
    private static readonly SolidColorBrush Vermelho = new(Color.FromRgb(0xC4, 0x32, 0x1A));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        TipoMovimentacao.EntregaInicial => Azul,
        TipoMovimentacao.Reposicao => Cinza,
        TipoMovimentacao.Troca => Laranja,
        TipoMovimentacao.Devolucao => Vermelho,
        _ => Cinza
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
