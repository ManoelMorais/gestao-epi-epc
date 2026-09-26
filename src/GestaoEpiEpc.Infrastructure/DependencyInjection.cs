using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Infrastructure.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a persistência em memória. Quando o banco de dados for conectado, este é o único
    /// método que muda: os repositórios passam a apontar para implementações com Entity Framework
    /// Core, mantendo as mesmas interfaces — Application e Desktop não precisam ser alterados.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
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

        return services;
    }
}
