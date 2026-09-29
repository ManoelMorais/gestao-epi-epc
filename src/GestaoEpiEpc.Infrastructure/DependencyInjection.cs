using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Infrastructure.InMemory;
using GestaoEpiEpc.Infrastructure.Persistence;
using GestaoEpiEpc.Infrastructure.Persistence.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a persistência. Com uma connection string, os repositórios usam o PostgreSQL do
    /// Supabase via Entity Framework Core; sem ela (ex.: nos testes), caem na implementação em memória.
    /// As interfaces são as mesmas nos dois casos — Application e Desktop não percebem a diferença.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return services.AddInfrastructureEmMemoria();

        services.AddDbContextFactory<GestaoEpiDbContext>(options => ConfigurarPostgres(options, connectionString));
        services.AddSingleton<InicializadorBancoDeDados>();

        services.AddSingleton<IUnidadeRepository, EfUnidadeRepository>();
        services.AddSingleton<ICargoRepository, EfCargoRepository>();
        services.AddSingleton<IColaboradorRepository, EfColaboradorRepository>();
        services.AddSingleton<ICategoriaItemRepository, EfCategoriaItemRepository>();
        services.AddSingleton<IItemEpiEpcRepository, EfItemEpiEpcRepository>();
        services.AddSingleton<ICargoItemPermitidoRepository, EfCargoItemPermitidoRepository>();
        services.AddSingleton<IMotivoMovimentacaoRepository, EfMotivoMovimentacaoRepository>();
        services.AddSingleton<IUsuarioRepository, EfUsuarioRepository>();
        services.AddSingleton<IEntregaRepository, EfEntregaRepository>();
        services.AddSingleton<ILogAuditoriaRepository, EfLogAuditoriaRepository>();
        services.AddSingleton<ISolicitacaoRepository, EfSolicitacaoRepository>();

        return services;
    }

    internal static void ConfigurarPostgres(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3))
            .UseSnakeCaseNamingConvention();

    private static IServiceCollection AddInfrastructureEmMemoria(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryDataStore>();

        services.AddSingleton<IUnidadeRepository, InMemoryUnidadeRepository>();
        services.AddSingleton<ICargoRepository, InMemoryCargoRepository>();
        services.AddSingleton<IColaboradorRepository, InMemoryColaboradorRepository>();
        services.AddSingleton<ICategoriaItemRepository, InMemoryCategoriaItemRepository>();
        services.AddSingleton<IItemEpiEpcRepository, InMemoryItemEpiEpcRepository>();
        services.AddSingleton<ICargoItemPermitidoRepository, InMemoryCargoItemPermitidoRepository>();
        services.AddSingleton<IMotivoMovimentacaoRepository, InMemoryMotivoMovimentacaoRepository>();
        services.AddSingleton<IUsuarioRepository, InMemoryUsuarioRepository>();
        services.AddSingleton<IEntregaRepository, InMemoryEntregaRepository>();
        services.AddSingleton<ILogAuditoriaRepository, InMemoryLogAuditoriaRepository>();
        services.AddSingleton<ISolicitacaoRepository, InMemorySolicitacaoRepository>();

        return services;
    }
}
