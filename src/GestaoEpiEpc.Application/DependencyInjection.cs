using GestaoEpiEpc.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IElegibilidadeService, ElegibilidadeService>();
        services.AddScoped<IEntregaService, EntregaService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();

        return services;
    }
}
