using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Views;

public partial class ColaboradoresView : UserControl
{
    public ColaboradoresView() => InitializeComponent();

    private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid || DataContext is not ColaboradoresViewModel viewModel)
            return;

        if (ItemsControl.ContainerFromElement(grid, (DependencyObject)e.OriginalSource) is DataGridRow { Item: Colaborador colaborador })
            viewModel.AbrirDetalhes(colaborador);
    }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ColaboradoresViewModel viewModel)
            viewModel.FecharDetalhesCommand.Execute(null);
    }

    private void PopupCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;
}
