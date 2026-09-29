using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestaoEpiEpc.Infrastructure.Persistence;

/// <summary>
/// Usada só pelas ferramentas do EF (<c>dotnet ef migrations add ...</c>). Gerar uma migration não
/// conecta no banco, então sem a variável de ambiente usamos uma connection string fictícia; para
/// rodar <c>dotnet ef database update</c> direto do terminal, defina <c>ConnectionStrings__Supabase</c>.
/// </summary>
public class GestaoEpiDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GestaoEpiDbContext>
{
    public GestaoEpiDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Supabase")
            ?? "Host=localhost;Database=gestao_epi;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<GestaoEpiDbContext>();
        DependencyInjection.ConfigurarPostgres(options, connectionString);
        return new GestaoEpiDbContext(options.Options);
    }
}
