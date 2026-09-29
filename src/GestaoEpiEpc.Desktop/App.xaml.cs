using System.Windows;
using GestaoEpiEpc.Application;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Desktop.ViewModels;
using GestaoEpiEpc.Infrastructure;
using GestaoEpiEpc.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestaoEpiEpc.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration(config => config
                // Connection string real fica fora do git; ver appsettings.Local.example.json.
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                // Variáveis de ambiente continuam valendo mais que os arquivos (ex.: apontar para um banco de teste).
                .AddEnvironmentVariables())
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(context.Configuration.GetConnectionString("Supabase"));

                services.AddSingleton<SessaoAtual>();

                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ColaboradoresViewModel>();
                services.AddTransient<CatalogoViewModel>();
                services.AddTransient<CargosElegibilidadeViewModel>();
                services.AddTransient<SolicitacoesViewModel>();
                services.AddTransient<UsuariosViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host.StartAsync();

        if (!await InicializarBancoDeDadosAsync())
        {
            Shutdown(1);
            return;
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();

        mainWindow.DataContext = mainViewModel;
        await mainViewModel.InicializarAsync();

        mainWindow.Show();
    }

    /// <summary>Aplica as migrations no Supabase (e popula um banco vazio). Sem connection string o app roda em memória.</summary>
    private async Task<bool> InicializarBancoDeDadosAsync()
    {
        var inicializador = _host.Services.GetService<InicializadorBancoDeDados>();
        if (inicializador is null) return true;

        try
        {
            await inicializador.InicializarAsync();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível conectar ao banco de dados (Supabase).\n\n" +
                "Confira a connection string em appsettings.Local.json e a conexão com a internet.\n\n" +
                $"Detalhe: {ex.GetBaseException().Message}",
                "Gestão de EPI/EPC", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
