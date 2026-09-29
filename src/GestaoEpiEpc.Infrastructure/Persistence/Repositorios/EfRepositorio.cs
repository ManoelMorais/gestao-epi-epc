using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace GestaoEpiEpc.Infrastructure.Persistence.Repositorios;

/// <summary>
/// Base dos repositórios com Entity Framework Core. Cada operação abre um <see cref="GestaoEpiDbContext"/>
/// próprio e curto (via <see cref="IDbContextFactory{TContext}"/>) — o padrão recomendado para apps
/// desktop, onde um contexto único e longo acumularia entidades rastreadas e dados desatualizados.
/// Por isso as entidades devolvidas vêm desanexadas (<c>AsNoTracking</c>) e as gravações anexam só a
/// entidade recebida, nunca o grafo de navegação que ela possa estar carregando.
/// </summary>
public abstract class EfRepositorio<T>(IDbContextFactory<GestaoEpiDbContext> fabrica) : IRepository<T>
    where T : EntidadeBase
{
    protected IDbContextFactory<GestaoEpiDbContext> Fabrica { get; } = fabrica;

    /// <summary>Consulta base com os Includes que a tela precisa exibir (ex.: cargo do colaborador).</summary>
    protected virtual IQueryable<T> Consulta(GestaoEpiDbContext db) => db.Set<T>().AsNoTracking();

    /// <summary>Ordenação padrão das listagens, espelhando a implementação em memória.</summary>
    protected virtual IQueryable<T> Ordenar(IQueryable<T> consulta) => consulta;

    public virtual async Task<T?> ObterPorIdAsync(Guid id)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db).FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IReadOnlyList<T>> ListarAsync()
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Ordenar(Consulta(db)).ToListAsync();
    }

    public virtual async Task AdicionarAsync(T entidade)
    {
        await using var db = await CriarContextoDeGravacaoAsync();
        db.Entry(entidade).State = EntityState.Added;
        await db.SaveChangesAsync();
    }

    public virtual async Task AtualizarAsync(T entidade)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        var existente = await db.Set<T>().FindAsync(entidade.Id);
        if (existente is null) return;

        // Copia só os valores escalares (nunca as navegações) para a versão rastreada do banco.
        var entry = db.Entry(existente);
        entry.CurrentValues.SetValues(entidade);
        // As telas montam um objeto novo ao editar, então CriadoEm chega com a hora atual — nunca sobrescrever.
        entry.Property(e => e.CriadoEm).IsModified = false;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Contexto para inserções sem detecção automática de mudanças: assim o EF grava exatamente as
    /// entidades marcadas como <c>Added</c>, sem tentar inserir de novo cargos, colaboradores etc.
    /// que por acaso estejam pendurados nas propriedades de navegação.
    /// </summary>
    protected async Task<GestaoEpiDbContext> CriarContextoDeGravacaoAsync()
    {
        var db = await Fabrica.CreateDbContextAsync();
        db.ChangeTracker.AutoDetectChangesEnabled = false;
        return db;
    }

    public virtual async Task RemoverAsync(Guid id)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        await db.Set<T>().Where(e => e.Id == id).ExecuteDeleteAsync();
    }
}
