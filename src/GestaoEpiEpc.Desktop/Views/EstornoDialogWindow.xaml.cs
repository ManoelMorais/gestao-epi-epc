using System.Windows;

namespace GestaoEpiEpc.Desktop.Views;

public partial class EstornoDialogWindow : Window
{
    public string Justificativa { get; private set; } = string.Empty;

    public EstornoDialogWindow() => InitializeComponent();

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtJustificativa.Text))
        {
            MessageBox.Show(this, "Informe a justificativa do estorno.", "Estornar entrega", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Justificativa = TxtJustificativa.Text.Trim();
        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    /// <summary>Mostra o diálogo e devolve a justificativa, ou null se o usuário cancelou.</summary>
    public static string? PedirJustificativa(Window owner)
    {
        var dialogo = new EstornoDialogWindow { Owner = owner };
        return dialogo.ShowDialog() == true ? dialogo.Justificativa : null;
    }
}
