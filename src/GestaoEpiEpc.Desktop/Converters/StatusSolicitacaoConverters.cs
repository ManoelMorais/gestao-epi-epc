using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestaoEpiEpc.Domain.Enums;
using MaterialDesignThemes.Wpf;

namespace GestaoEpiEpc.Desktop.Converters;

/// <summary>Mesmas cores de status do app mobile, para as duas pontas "falarem a mesma língua".</summary>
public class StatusSolicitacaoParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Pendente = Congelado(0xE8, 0x5D, 0x19);
    private static readonly SolidColorBrush EmAnalise = Congelado(0x6D, 0x28, 0xD9);
    private static readonly SolidColorBrush Aprovada = Congelado(0x1D, 0x4E, 0xD8);
    private static readonly SolidColorBrush Entregue = Congelado(0x1E, 0x82, 0x4C);
    private static readonly SolidColorBrush Recusada = Congelado(0xC4, 0x32, 0x1A);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StatusSolicitacao.EmAnalise => EmAnalise,
        StatusSolicitacao.Aprovada => Aprovada,
        StatusSolicitacao.Entregue => Entregue,
        StatusSolicitacao.Recusada => Recusada,
        _ => Pendente
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    internal static SolidColorBrush Congelado(byte r, byte g, byte b)
    {
        var pincel = new SolidColorBrush(Color.FromRgb(r, g, b));
        pincel.Freeze();
        return pincel;
    }
}

public class StatusSolicitacaoParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StatusSolicitacao.Pendente => "Pendente",
        StatusSolicitacao.EmAnalise => "Em análise",
        StatusSolicitacao.Aprovada => "Aprovada",
        StatusSolicitacao.Entregue => "Entregue",
        StatusSolicitacao.Recusada => "Recusada",
        _ => string.Empty
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class StatusSolicitacaoParaIconeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        StatusSolicitacao.Pendente => PackIconKind.SendOutline,
        StatusSolicitacao.EmAnalise => PackIconKind.MagnifyScan,
        StatusSolicitacao.Aprovada => PackIconKind.CheckDecagramOutline,
        StatusSolicitacao.Entregue => PackIconKind.PackageVariantClosedCheck,
        StatusSolicitacao.Recusada => PackIconKind.CloseOctagonOutline,
        _ => PackIconKind.CircleOutline
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Dias para vencer → texto curto do chip de validade ("Vencido há 20 dias", "Vence em 12 dias", "Sem validade").</summary>
public class DiasParaVencerParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        int d when d < 0 => $"Vencido há {-d} {(d == -1 ? "dia" : "dias")}",
        0 => "Vence hoje",
        int d when d <= 30 => $"Vence em {d} {(d == 1 ? "dia" : "dias")}",
        int => "Em dia",
        _ => "Sem validade"
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class DiasParaVencerParaCorConverter : IValueConverter
{
    private static readonly SolidColorBrush Vencido = StatusSolicitacaoParaCorConverter.Congelado(0xC4, 0x32, 0x1A);
    private static readonly SolidColorBrush Atencao = StatusSolicitacaoParaCorConverter.Congelado(0xE8, 0x5D, 0x19);
    private static readonly SolidColorBrush EmDia = StatusSolicitacaoParaCorConverter.Congelado(0x1E, 0x82, 0x4C);
    private static readonly SolidColorBrush SemValidade = StatusSolicitacaoParaCorConverter.Congelado(0x4B, 0x55, 0x66);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        int d when d < 0 => Vencido,
        int d when d <= 30 => Atencao,
        int => EmDia,
        _ => SemValidade
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Converte a assinatura desenhada no app (caminhos SVG "M20 80 C 35 30, ...") em geometria WPF.
/// A mini-linguagem de path do WPF é praticamente a mesma do SVG; vírgulas viram espaços por segurança.
/// </summary>
public class SvgParaGeometriaConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string caminho || string.IsNullOrWhiteSpace(caminho)) return null;
        try
        {
            var geometria = Geometry.Parse(caminho.Replace(',', ' '));
            geometria.Freeze();
            return geometria;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
