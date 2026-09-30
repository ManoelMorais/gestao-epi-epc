using System.Windows;
using System.Windows.Controls;

namespace GestaoEpiEpc.Desktop.Common;

/// <summary>
/// Escolhe o template do conteúdo dos botões nomeados (Themes/Styles.xaml, BotaoBase): texto puro
/// ganha <see cref="ModeloTexto"/> (TextBlock com a cor ligada à do botão); qualquer outro conteúdo
/// (ex.: ícone + texto montados na view) volta <c>null</c> e é exibido normalmente pelo WPF.
/// Um ContentTemplate fixo não serve: seria aplicado também ao conteúdo montado e o transformaria
/// em "System.Windows.Controls.StackPanel".
/// </summary>
public class SeletorConteudoBotao : DataTemplateSelector
{
    public DataTemplate? ModeloTexto { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) =>
        item is string ? ModeloTexto : null;
}
