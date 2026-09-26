using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Views;

public partial class EntregasView : UserControl
{
    public EntregasView() => InitializeComponent();

    private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid || DataContext is not EntregasViewModel viewModel)
            return;

        if (ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow { Item: Entrega entrega })
            viewModel.AbrirDetalhes(entrega);
    }

    private async void Estornar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not EntregasViewModel viewModel || viewModel.EntregaSelecionada is not { } entrega)
            return;

        var owner = Window.GetWindow(this);
        var justificativa = owner is null ? null : EstornoDialogWindow.PedirJustificativa(owner);
        if (justificativa is null) return;

        await viewModel.EstornarAsync(entrega, justificativa);
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is EntregasViewModel viewModel)
            viewModel.FecharDetalhesCommand.Execute(null);
    }

    private void PopupCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;
}
