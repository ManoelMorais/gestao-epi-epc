using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Views;

public partial class AuditoriaView : UserControl
{
    public AuditoriaView() => InitializeComponent();

    private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid || DataContext is not AuditoriaViewModel viewModel)
            return;

        if (ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow { Item: LogAuditoria log })
            viewModel.AbrirDetalhes(log);
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is AuditoriaViewModel viewModel)
            viewModel.FecharDetalhesCommand.Execute(null);
    }

    private void PopupCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;
}
