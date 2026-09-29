using System.Windows;

namespace GestaoEpiEpc.Desktop.Views;

/// <summary>Diálogo simples que pede um texto ao usuário (motivo de recusa, observação da aprovação...).</summary>
public partial class DialogoTextoWindow : Window
{
    private bool _obrigatorio;

    public string Resposta { get; private set; } = string.Empty;

    public DialogoTextoWindow() => InitializeComponent();

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (_obrigatorio && string.IsNullOrWhiteSpace(TxtResposta.Text))
        {
            MessageBox.Show(this, "Preencha o campo antes de confirmar.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Resposta = TxtResposta.Text.Trim();
        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    /// <summary>Mostra o diálogo e devolve o texto digitado (vazio se opcional e em branco), ou null se cancelado.</summary>
    public static string? Pedir(Window owner, string titulo, string descricao, string textoConfirmar, bool obrigatorio, bool perigo = false)
    {
        var dialogo = new DialogoTextoWindow { Owner = owner, Title = titulo, _obrigatorio = obrigatorio };
        dialogo.TxtTitulo.Text = titulo;
        dialogo.TxtDescricao.Text = descricao;
        dialogo.BtnConfirmar.Content = textoConfirmar;
        if (perigo) dialogo.BtnConfirmar.Style = (Style)dialogo.FindResource("BotaoPerigo");
        dialogo.TxtResposta.Focus();
        return dialogo.ShowDialog() == true ? dialogo.Resposta : null;
    }
}
