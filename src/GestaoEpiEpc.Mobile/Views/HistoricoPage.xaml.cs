using GestaoEpiEpc.Mobile.ViewModels;

namespace GestaoEpiEpc.Mobile.Views;

public partial class HistoricoPage : ContentPage
{
    private readonly HistoricoViewModel _viewModel;

    public HistoricoPage(HistoricoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarAsync();
    }
}
