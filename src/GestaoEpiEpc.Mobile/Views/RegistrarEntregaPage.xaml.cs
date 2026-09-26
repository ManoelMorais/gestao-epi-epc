using GestaoEpiEpc.Mobile.ViewModels;

namespace GestaoEpiEpc.Mobile.Views;

public partial class RegistrarEntregaPage : ContentPage
{
    private readonly RegistrarEntregaViewModel _viewModel;

    public RegistrarEntregaPage(RegistrarEntregaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InicializarAsync();
    }
}
