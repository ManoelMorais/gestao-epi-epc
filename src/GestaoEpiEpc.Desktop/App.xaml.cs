using System.Windows;
using GestaoEpiEpc.Application;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestaoEpiEpc.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure();

                services.AddSingleton<SessaoAtual>();

                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ColaboradoresViewModel>();
                services.AddTransient<CatalogoViewModel>();
                services.AddTransient<CargosElegibilidadeViewModel>();
                services.AddTransient<EntregasViewModel>();
                services.AddTransient<UsuariosViewModel>();
                services.AddTransient<AuditoriaViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

        mainWindow.DataContext = mainViewModel;
        await mainViewModel.InicializarAsync();

        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
