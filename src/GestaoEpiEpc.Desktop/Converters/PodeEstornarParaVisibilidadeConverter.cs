using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

/// <summary>[status da entrega, usuário pode estornar] → o botão só aparece para entregas confirmadas e para o Administrador.</summary>
public class PodeEstornarParaVisibilidadeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values is [StatusEntrega.Confirmada, true] ? Visibility.Visible : Visibility.Collapsed;

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
