using GestaoEpiEpc.Application;
using GestaoEpiEpc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Tests;

/// <summary>Monta um container de DI igual ao do app (Application + Infrastructure em memória),
/// já com os dados de exemplo do <c>DataSeeder</c> — uma instância nova por teste, para isolamento.</summary>
internal static class Fixture
{
    public static ServiceProvider CriarProvider()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();
        return services.BuildServiceProvider();
    }
}
