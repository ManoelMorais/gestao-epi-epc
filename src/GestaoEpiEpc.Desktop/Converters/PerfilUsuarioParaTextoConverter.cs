using System.Globalization;
using System.Windows.Data;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.Converters;

public class PerfilUsuarioParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        PerfilUsuario.Facilitador => "Facilitador",
        PerfilUsuario.Gestao => "Gestão",
        PerfilUsuario.SegurancaTrabalho => "Segurança do Trabalho",
        PerfilUsuario.Rh => "RH",
        PerfilUsuario.Administrador => "Administrador",
        _ => value?.ToString() ?? string.Empty
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
