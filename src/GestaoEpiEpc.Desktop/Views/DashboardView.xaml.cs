using System.Windows;
using System.Windows.Controls;
using GestaoEpiEpc.Desktop.ViewModels;

namespace GestaoEpiEpc.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView() => InitializeComponent();

    private void GraficoArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
            vm.AtualizarLarguraGrafico(e.NewSize.Width);
    }
}
