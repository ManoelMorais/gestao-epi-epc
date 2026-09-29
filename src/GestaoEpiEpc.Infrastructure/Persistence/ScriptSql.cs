using System.Reflection;

namespace GestaoEpiEpc.Infrastructure.Persistence;

/// <summary>Lê os scripts .sql embutidos em Persistence/Sql (usados pelas migrations).</summary>
internal static class ScriptSql
{
    public static string Ler(string arquivo)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var nome = assembly.GetManifestResourceNames().Single(n => n.EndsWith("." + arquivo, StringComparison.Ordinal));
        using var leitor = new StreamReader(assembly.GetManifestResourceStream(nome)!);
        return leitor.ReadToEnd();
    }
}
