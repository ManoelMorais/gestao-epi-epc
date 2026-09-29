using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestaoEpiEpc.Infrastructure.Persistence;

/// <summary>
/// Banco de dados real do sistema (PostgreSQL hospedado no Supabase). Tabelas e colunas seguem o
/// padrão snake_case do Postgres (ex.: <c>cargo_item_permitido.cargo_id</c>) e os enums são gravados
/// como texto, para que os dados fiquem legíveis direto no painel do Supabase.
/// </summary>
public class GestaoEpiDbContext(DbContextOptions<GestaoEpiDbContext> options) : DbContext(options)
{
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<CategoriaItem> CategoriasItem => Set<CategoriaItem>();
    public DbSet<ItemEpiEpc> Itens => Set<ItemEpiEpc>();
    public DbSet<CargoItemPermitido> CargoItemPermitidos => Set<CargoItemPermitido>();
    public DbSet<MotivoMovimentacao> MotivosMovimentacao => Set<MotivoMovimentacao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Entrega> Entregas => Set<Entrega>();
    public DbSet<EntregaItem> EntregaItens => Set<EntregaItem>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<EventoSolicitacao> EventosSolicitacao => Set<EventoSolicitacao>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // O domínio usa DateTime.Now (horário local); guardamos sem fuso para ler de volta exatamente o que foi gravado.
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");

        configurationBuilder.Properties<PerfilUsuario>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<StatusColaborador>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<StatusEntrega>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<TipoItem>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<TipoMovimentacao>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<StatusSolicitacao>().HaveConversion<string>().HaveMaxLength(30);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Unidade>(e =>
        {
            e.ToTable("unidade");
            e.Property(u => u.Id).ValueGeneratedNever();
            e.Property(u => u.Nome).HasMaxLength(120);
            e.Property(u => u.Sigla).HasMaxLength(10);
            e.Property(u => u.Cidade).HasMaxLength(120);
        });

        modelBuilder.Entity<Cargo>(e =>
        {
            e.ToTable("cargo");
            e.Property(c => c.Id).ValueGeneratedNever();
            e.Property(c => c.Nome).HasMaxLength(120);
            e.HasIndex(c => c.Nome).IsUnique();
        });

        modelBuilder.Entity<Colaborador>(e =>
        {
            e.ToTable("colaborador");
            e.Property(c => c.Id).ValueGeneratedNever();
            e.Property(c => c.Drt).HasMaxLength(20);
            e.Property(c => c.Nome).HasMaxLength(150);
            e.Property(c => c.Area).HasMaxLength(120);
            e.Property(c => c.Email).HasMaxLength(150);
            e.Property(c => c.Telefone).HasMaxLength(20);
            e.Property(c => c.SenhaHash).HasMaxLength(200);
            e.HasIndex(c => c.Drt).IsUnique();
            e.HasOne(c => c.Cargo).WithMany().HasForeignKey(c => c.CargoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Unidade).WithMany().HasForeignKey(c => c.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CategoriaItem>(e =>
        {
            e.ToTable("categoria_item");
            e.Property(c => c.Id).ValueGeneratedNever();
            e.Property(c => c.Codigo).HasMaxLength(40);
            e.HasIndex(c => c.Codigo).IsUnique();
            e.Property(c => c.Nome).HasMaxLength(120);
        });

        modelBuilder.Entity<ItemEpiEpc>(e =>
        {
            e.ToTable("item_epi_epc");
            e.Property(i => i.Id).ValueGeneratedNever();
            e.Property(i => i.Codigo).HasMaxLength(20);
            e.Property(i => i.Nome).HasMaxLength(150);
            e.Property(i => i.NumeroCa).HasMaxLength(20);
            e.HasIndex(i => i.Codigo).IsUnique();
            e.HasOne(i => i.Categoria).WithMany().HasForeignKey(i => i.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CargoItemPermitido>(e =>
        {
            e.ToTable("cargo_item_permitido");
            e.Property(p => p.Id).ValueGeneratedNever();
            // A regra de elegibilidade: um mesmo item só pode ser permitido uma vez por cargo.
            e.HasIndex(p => new { p.CargoId, p.ItemId }).IsUnique();
            e.HasOne(p => p.Cargo).WithMany().HasForeignKey(p => p.CargoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Item).WithMany().HasForeignKey(p => p.ItemId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MotivoMovimentacao>(e =>
        {
            e.ToTable("motivo_movimentacao");
            e.Property(m => m.Id).ValueGeneratedNever();
            e.Property(m => m.Codigo).HasMaxLength(40);
            e.HasIndex(m => m.Codigo).IsUnique();
            e.Property(m => m.Descricao).HasMaxLength(150);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.Property(u => u.Id).ValueGeneratedNever();
            e.Property(u => u.Nome).HasMaxLength(150);
            e.Property(u => u.Email).HasMaxLength(150);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Unidade).WithMany().HasForeignKey(u => u.UnidadeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Entrega>(e =>
        {
            e.ToTable("entrega");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.AssinaturaUrl).HasMaxLength(500);
            e.Property(x => x.Observacao).HasMaxLength(1000);
            e.HasIndex(x => x.DataHora);
            e.HasOne(x => x.Colaborador).WithMany().HasForeignKey(x => x.ColaboradorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Facilitador).WithMany().HasForeignKey(x => x.FacilitadorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Unidade).WithMany().HasForeignKey(x => x.UnidadeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Motivo).WithMany().HasForeignKey(x => x.MotivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EntregaOrigem).WithMany().HasForeignKey(x => x.EntregaOrigemId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Itens).WithOne(i => i.Entrega).HasForeignKey(i => i.EntregaId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EntregaItem>(e =>
        {
            e.ToTable("entrega_item");
            e.Property(i => i.Id).ValueGeneratedNever();
            e.Property(i => i.Tamanho).HasMaxLength(10);
            e.Property(i => i.NumeroSerie).HasMaxLength(60);
            e.Property(i => i.FotoUrl).HasMaxLength(500);
            e.HasOne(i => i.Item).WithMany().HasForeignKey(i => i.ItemId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LogAuditoria>(e =>
        {
            e.ToTable("log_auditoria");
            e.Property(l => l.Id).ValueGeneratedNever();
            e.Property(l => l.Entidade).HasMaxLength(60);
            e.Property(l => l.Acao).HasMaxLength(200);
            e.HasIndex(l => l.DataHora);
            e.HasOne(l => l.Usuario).WithMany().HasForeignKey(l => l.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Solicitacao>(e =>
        {
            e.ToTable("solicitacao");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Protocolo).HasMaxLength(20);
            e.Property(x => x.Tamanho).HasMaxLength(10);
            e.Property(x => x.MaterialFabricacao).HasMaxLength(7);
            e.Property(x => x.MaterialMarca).HasMaxLength(80);
            e.Property(x => x.MaterialLote).HasMaxLength(40);
            e.Property(x => x.Relato).HasMaxLength(1000);
            // Fotos são URLs (Supabase Storage, quando houver upload), guardadas como text[] do Postgres.
            e.HasIndex(x => x.Protocolo).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CriadaEm);
            e.HasOne(x => x.Colaborador).WithMany().HasForeignKey(x => x.ColaboradorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Motivo).WithMany().HasForeignKey(x => x.MotivoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Entrega).WithMany().HasForeignKey(x => x.EntregaId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Historico).WithOne(h => h.Solicitacao).HasForeignKey(h => h.SolicitacaoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventoSolicitacao>(e =>
        {
            e.ToTable("evento_solicitacao");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Responsavel).HasMaxLength(150);
            e.Property(x => x.Comentario).HasMaxLength(1000);
        });
    }
}
