using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Views;

public partial class UsuariosView : UserControl
{
    public UsuariosView() => InitializeComponent();

    private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid || DataContext is not UsuariosViewModel viewModel)
            return;

        if (ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow { Item: Usuario usuario })
            viewModel.AbrirDetalhes(usuario);
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is UsuariosViewModel viewModel)
            viewModel.FecharDetalhesCommand.Execute(null);
    }

    private void PopupCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;
}
