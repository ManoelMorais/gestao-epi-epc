using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoEpiEpc.Infrastructure.Persistence.Repositorios;

public class EfUnidadeRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Unidade>(fabrica), IUnidadeRepository
{
    protected override IQueryable<Unidade> Ordenar(IQueryable<Unidade> consulta) => consulta.OrderBy(u => u.Nome);
}

public class EfCargoRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Cargo>(fabrica), ICargoRepository
{
    protected override IQueryable<Cargo> Ordenar(IQueryable<Cargo> consulta) => consulta.OrderBy(c => c.Nome);
}

public class EfCategoriaItemRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<CategoriaItem>(fabrica), ICategoriaItemRepository
{
    protected override IQueryable<CategoriaItem> Ordenar(IQueryable<CategoriaItem> consulta) => consulta.OrderBy(c => c.Nome);
}

public class EfMotivoMovimentacaoRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<MotivoMovimentacao>(fabrica), IMotivoMovimentacaoRepository
{
    protected override IQueryable<MotivoMovimentacao> Ordenar(IQueryable<MotivoMovimentacao> consulta) => consulta.OrderBy(m => m.Descricao);
}

public class EfColaboradorRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Colaborador>(fabrica), IColaboradorRepository
{
    protected override IQueryable<Colaborador> Consulta(GestaoEpiDbContext db) =>
        base.Consulta(db).Include(c => c.Cargo).Include(c => c.Unidade);

    protected override IQueryable<Colaborador> Ordenar(IQueryable<Colaborador> consulta) => consulta.OrderBy(c => c.Nome);

    public async Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo)
    {
        var padrao = $"%{termo.Trim()}%";
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db)
            .Where(c => EF.Functions.ILike(c.Nome, padrao) || EF.Functions.ILike(c.Drt, padrao))
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<Colaborador?> ObterPorDrtAsync(string drt)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db).FirstOrDefaultAsync(c => c.Drt == drt);
    }
}

public class EfItemEpiEpcRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<ItemEpiEpc>(fabrica), IItemEpiEpcRepository
{
    protected override IQueryable<ItemEpiEpc> Consulta(GestaoEpiDbContext db) =>
        base.Consulta(db).Include(i => i.Categoria);

    protected override IQueryable<ItemEpiEpc> Ordenar(IQueryable<ItemEpiEpc> consulta) => consulta.OrderBy(i => i.Nome);

    public async Task<IReadOnlyList<ItemEpiEpc>> BuscarAsync(string termo)
    {
        var padrao = $"%{termo.Trim()}%";
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db)
            .Where(i => EF.Functions.ILike(i.Nome, padrao) || EF.Functions.ILike(i.Codigo, padrao))
            .OrderBy(i => i.Nome)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ItemEpiEpc>> ListarPorIdsAsync(IEnumerable<Guid> ids)
    {
        var lista = ids.ToList();
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db).Where(i => lista.Contains(i.Id)).OrderBy(i => i.Nome).ToListAsync();
    }
}

public class EfCargoItemPermitidoRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<CargoItemPermitido>(fabrica), ICargoItemPermitidoRepository
{
    public async Task<IReadOnlyList<CargoItemPermitido>> ListarPorCargoAsync(Guid cargoId)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db).Where(p => p.CargoId == cargoId).ToListAsync();
    }

    public async Task RemoverPorCargoAsync(Guid cargoId)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        await db.CargoItemPermitidos.Where(p => p.CargoId == cargoId).ExecuteDeleteAsync();
    }
}

public class EfUsuarioRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Usuario>(fabrica), IUsuarioRepository
{
    protected override IQueryable<Usuario> Ordenar(IQueryable<Usuario> consulta) => consulta.OrderBy(u => u.Nome);

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db).FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }
}

public class EfLogAuditoriaRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<LogAuditoria>(fabrica), ILogAuditoriaRepository
{
    protected override IQueryable<LogAuditoria> Ordenar(IQueryable<LogAuditoria> consulta) => consulta.OrderByDescending(l => l.DataHora);

    public Task RegistrarAsync(Guid usuarioId, string entidade, Guid entidadeId, string acao, string? dadosAntes = null, string? dadosDepois = null) =>
        AdicionarAsync(new LogAuditoria
        {
            UsuarioId = usuarioId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            DadosAntes = dadosAntes,
            DadosDepois = dadosDepois
        });

    public async Task<IReadOnlyList<LogAuditoria>> ConsultarAsync(Guid? usuarioId, string? entidade, DateOnly? de, DateOnly? ate)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        var consulta = Consulta(db).Include(l => l.Usuario).AsQueryable();

        if (usuarioId is { } uid) consulta = consulta.Where(l => l.UsuarioId == uid);
        if (!string.IsNullOrWhiteSpace(entidade)) consulta = consulta.Where(l => l.Entidade == entidade);
        if (de is { } dataInicio)
        {
            var inicio = dataInicio.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(l => l.DataHora >= inicio);
        }
        if (ate is { } dataFim)
        {
            var fimExclusivo = dataFim.AddDays(1).ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(l => l.DataHora < fimExclusivo);
        }

        return await consulta.OrderByDescending(l => l.DataHora).ToListAsync();
    }
}

public class EfEntregaRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Entrega>(fabrica), IEntregaRepository
{
    /// <summary>Carrega tudo o que as telas e o dashboard exibem de uma entrega.</summary>
    protected override IQueryable<Entrega> Consulta(GestaoEpiDbContext db) =>
        base.Consulta(db)
            .Include(e => e.Colaborador).ThenInclude(c => c!.Cargo)
            .Include(e => e.Colaborador).ThenInclude(c => c!.Unidade)
            .Include(e => e.Facilitador)
            .Include(e => e.Unidade)
            .Include(e => e.Motivo)
            .Include(e => e.Itens).ThenInclude(i => i.Item).ThenInclude(i => i!.Categoria)
            .AsSplitQuery();

    protected override IQueryable<Entrega> Ordenar(IQueryable<Entrega> consulta) => consulta.OrderByDescending(e => e.DataHora);

    public async Task<IReadOnlyList<Entrega>> ConsultarAsync(FiltroEntregas filtro)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        var consulta = Consulta(db);

        if (filtro.DataInicio is { } dataInicio)
        {
            var inicio = dataInicio.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(e => e.DataHora >= inicio);
        }

        if (filtro.DataFim is { } dataFim)
        {
            var fimExclusivo = dataFim.AddDays(1).ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(e => e.DataHora < fimExclusivo);
        }

        if (filtro.ColaboradorId is { } colaboradorId)
            consulta = consulta.Where(e => e.ColaboradorId == colaboradorId);

        if (filtro.FacilitadorId is { } facilitadorId)
            consulta = consulta.Where(e => e.FacilitadorId == facilitadorId);

        if (filtro.TipoMovimentacao is { } tipo)
            consulta = consulta.Where(e => e.TipoMovimentacao == tipo);

        if (filtro.UnidadeId is { } unidadeId)
            consulta = consulta.Where(e => e.UnidadeId == unidadeId);

        if (filtro.ItemId is { } itemId)
            consulta = consulta.Where(e => e.Itens.Any(i => i.ItemId == itemId));

        if (filtro.CategoriaId is { } categoriaId)
            consulta = consulta.Where(e => e.Itens.Any(i => i.Item!.CategoriaId == categoriaId));

        if (!string.IsNullOrWhiteSpace(filtro.Setor))
        {
            var setor = filtro.Setor.Trim().ToLower();
            consulta = consulta.Where(e => e.Colaborador!.Area.ToLower() == setor);
        }

        return await consulta.OrderByDescending(e => e.DataHora).ToListAsync();
    }

    public async Task<IReadOnlyList<Entrega>> ListarPorColaboradorAsync(Guid colaboradorId)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        return await Consulta(db)
            .Where(e => e.ColaboradorId == colaboradorId)
            .OrderByDescending(e => e.DataHora)
            .ToListAsync();
    }

    /// <summary>Grava o cabeçalho e os itens juntos, numa única transação.</summary>
    public override async Task AdicionarAsync(Entrega entidade)
    {
        await using var db = await CriarContextoDeGravacaoAsync();
        db.Entry(entidade).State = EntityState.Added;
        foreach (var item in entidade.Itens)
            db.Entry(item).State = EntityState.Added;

        await db.SaveChangesAsync();
    }
}

public class EfSolicitacaoRepository(IDbContextFactory<GestaoEpiDbContext> fabrica)
    : EfRepositorio<Solicitacao>(fabrica), ISolicitacaoRepository
{
    protected override IQueryable<Solicitacao> Consulta(GestaoEpiDbContext db) =>
        base.Consulta(db)
            .Include(s => s.Colaborador).ThenInclude(c => c!.Cargo)
            .Include(s => s.Colaborador).ThenInclude(c => c!.Unidade)
            .Include(s => s.Item).ThenInclude(i => i!.Categoria)
            .Include(s => s.Motivo)
            .Include(s => s.Historico.OrderBy(h => h.DataHora))
            .AsSplitQuery();

    protected override IQueryable<Solicitacao> Ordenar(IQueryable<Solicitacao> consulta) => consulta.OrderByDescending(s => s.CriadaEm);

    public async Task<IReadOnlyList<Solicitacao>> ConsultarAsync(FiltroSolicitacoes filtro)
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        var consulta = Consulta(db);

        if (filtro.Status is { Count: > 0 } status)
        {
            var lista = status.ToList();
            consulta = consulta.Where(s => lista.Contains(s.Status));
        }

        if (filtro.ColaboradorId is { } colaboradorId)
            consulta = consulta.Where(s => s.ColaboradorId == colaboradorId);

        if (filtro.UnidadeId is { } unidadeId)
            consulta = consulta.Where(s => s.Colaborador!.UnidadeId == unidadeId);

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var padrao = $"%{filtro.Termo.Trim()}%";
            consulta = consulta.Where(s =>
                EF.Functions.ILike(s.Protocolo, padrao) || EF.Functions.ILike(s.Colaborador!.Nome, padrao) ||
                EF.Functions.ILike(s.Colaborador!.Drt, padrao) || EF.Functions.ILike(s.Item!.Nome, padrao));
        }

        return await consulta.OrderByDescending(s => s.CriadaEm).ToListAsync();
    }

    public async Task AdicionarEventoAsync(EventoSolicitacao evento)
    {
        await using var db = await CriarContextoDeGravacaoAsync();
        db.Entry(evento).State = EntityState.Added;
        await db.SaveChangesAsync();
    }

    public async Task<int> ObterUltimoSequencialProtocoloAsync()
    {
        await using var db = await Fabrica.CreateDbContextAsync();
        var protocolos = await db.Solicitacoes.Select(s => s.Protocolo).ToListAsync();
        return protocolos.Select(InMemory.InMemorySolicitacaoRepository.SequencialDoProtocolo).DefaultIfEmpty(0).Max();
    }

    /// <summary>Grava a solicitação e o histórico que vier junto numa única transação.</summary>
    public override async Task AdicionarAsync(Solicitacao entidade)
    {
        await using var db = await CriarContextoDeGravacaoAsync();
        db.Entry(entidade).State = EntityState.Added;
        foreach (var evento in entidade.Historico)
            db.Entry(evento).State = EntityState.Added;

        await db.SaveChangesAsync();
    }
}
