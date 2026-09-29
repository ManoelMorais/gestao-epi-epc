using GestaoEpiEpc.Application.Seguranca;
using GestaoEpiEpc.Infrastructure.InMemory;
using GestaoEpiEpc.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

namespace GestaoEpiEpc.Infrastructure.Persistence;

/// <summary>
/// Executado na abertura do app: cria/atualiza o schema no Supabase aplicando as migrations pendentes
/// e, se o banco ainda estiver vazio, grava os mesmos dados de exemplo do <c>DataSeeder</c> — assim a
/// primeira execução contra um projeto Supabase novo já abre com o sistema demonstrável.
/// </summary>
public class InicializadorBancoDeDados(IDbContextFactory<GestaoEpiDbContext> fabrica)
{
    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(cancellationToken);

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Unidades.AnyAsync(cancellationToken))
        {
            await AtualizarHashesLegadosAsync(db, cancellationToken);
            return;
        }

        // Reaproveita o DataSeeder: ele popula um store em memória, que então é copiado para o banco.
        var exemplo = new InMemoryDataStore();

        db.Unidades.AddRange(exemplo.Unidades);
        db.Cargos.AddRange(exemplo.Cargos);
        db.CategoriasItem.AddRange(exemplo.CategoriasItem);
        db.Itens.AddRange(exemplo.Itens);
        db.CargoItemPermitidos.AddRange(exemplo.CargoItemPermitidos);
        db.MotivosMovimentacao.AddRange(exemplo.MotivosMovimentacao);
        db.Usuarios.AddRange(exemplo.Usuarios);
        db.Colaboradores.AddRange(exemplo.Colaboradores);
        db.Entregas.AddRange(exemplo.Entregas);
        db.LogsAuditoria.AddRange(exemplo.LogsAuditoria);
        db.Solicitacoes.AddRange(exemplo.Solicitacoes);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// O login do app é verificado no próprio Postgres, que só entende bcrypt. Hashes PBKDF2 antigos
    /// não podem ser convertidos sem a senha — mas os de demonstração (senha conhecida) sim: se o hash
    /// confere com a senha demo, regravamos em bcrypt. Qualquer outro continua como está (e precisaria de
    /// redefinição de senha).
    /// </summary>
    private static async Task AtualizarHashesLegadosAsync(GestaoEpiDbContext db, CancellationToken cancellationToken)
    {
        var legados = await db.Colaboradores.Where(c => c.SenhaHash!.StartsWith("pbkdf2$")).ToListAsync(cancellationToken);
        var atualizados = legados.Where(c => HashSenha.Verificar(DataSeeder.SenhaDemo, c.SenhaHash)).ToList();
        if (atualizados.Count == 0) return;

        var novoHash = HashSenha.Gerar(DataSeeder.SenhaDemo);
        foreach (var colaborador in atualizados)
            colaborador.SenhaHash = novoHash;

        await db.SaveChangesAsync(cancellationToken);
    }
}
