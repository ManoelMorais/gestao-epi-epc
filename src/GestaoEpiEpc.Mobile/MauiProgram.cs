using GestaoEpiEpc.Application;
using GestaoEpiEpc.Infrastructure;
using GestaoEpiEpc.Mobile.Common;
using GestaoEpiEpc.Mobile.ViewModels;
using GestaoEpiEpc.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace GestaoEpiEpc.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();

        builder.Services.AddSingleton<SessaoAtual>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<RegistrarEntregaViewModel>();
        builder.Services.AddTransient<RegistrarEntregaPage>();
        builder.Services.AddTransient<HistoricoViewModel>();
        builder.Services.AddTransient<HistoricoPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
