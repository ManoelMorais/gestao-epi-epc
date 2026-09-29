using GestaoEpiEpc.Application;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using GestaoEpiEpc.Infrastructure;
using GestaoEpiEpc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GestaoEpiEpc.Tests;

/// <summary>
/// Só roda quando a variável de ambiente <c>GESTAOEPI_TESTE_POSTGRES</c> aponta para um PostgreSQL
/// (ex.: um container local). Senão os testes aparecem como ignorados — a suíte normal continua em memória.
/// </summary>
public sealed class FactPostgresAttribute : FactAttribute
{
    public const string VariavelAmbiente = "GESTAOEPI_TESTE_POSTGRES";

    public FactPostgresAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariavelAmbiente)))
            Skip = $"Defina {VariavelAmbiente} com uma connection string de PostgreSQL para rodar.";
    }
}

/// <summary>
/// Exercita os serviços reais contra o PostgreSQL (mesma engine do Supabase), passando pelos
/// repositórios EF, pela migration e pelo seed. Cada teste usa um banco novo, apagado no final.
/// </summary>
public class PostgresIntegracaoTests : IAsyncLifetime
{
    private ServiceProvider? _provider;

    private ServiceProvider Provider => _provider!;

    public async Task InitializeAsync()
    {
        var baseConexao = Environment.GetEnvironmentVariable(FactPostgresAttribute.VariavelAmbiente);
        if (string.IsNullOrWhiteSpace(baseConexao)) return;

        var conexao = new NpgsqlConnectionStringBuilder(baseConexao)
        {
            Database = $"gestao_epi_teste_{Guid.NewGuid():N}"
        }.ConnectionString;

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(conexao);
        _provider = services.BuildServiceProvider();

        await Provider.GetRequiredService<InicializadorBancoDeDados>().InicializarAsync();
    }

    public async Task DisposeAsync()
    {
        if (_provider is null) return;

        await using (var db = await Provider.GetRequiredService<IDbContextFactory<GestaoEpiDbContext>>().CreateDbContextAsync())
            await db.Database.EnsureDeletedAsync();

        await _provider.DisposeAsync();
    }

    [FactPostgres]
    public async Task Seed_PopulaBancoVazio_ComOsDadosDeExemplo()
    {
        Assert.Equal(2, (await Provider.GetRequiredService<IUnidadeRepository>().ListarAsync()).Count);
        Assert.Equal(18, (await Provider.GetRequiredService<IColaboradorService>().ListarAsync()).Count);
        Assert.Equal(12, (await Provider.GetRequiredService<ICatalogoService>().ListarItensAsync()).Count);
        Assert.True((await Provider.GetRequiredService<IEntregaService>().ConsultarAsync(new FiltroEntregas())).Count > 50);
    }

    [FactPostgres]
    public async Task Inicializador_RodandoDeNovo_NaoDuplicaDados()
    {
        await Provider.GetRequiredService<InicializadorBancoDeDados>().InicializarAsync();

        Assert.Equal(18, (await Provider.GetRequiredService<IColaboradorService>().ListarAsync()).Count);
    }

    [FactPostgres]
    public async Task ConsultarEntregas_CarregaNavegacoesUsadasPelasTelas()
    {
        var entregas = await Provider.GetRequiredService<IEntregaService>().ConsultarAsync(new FiltroEntregas());

        Assert.All(entregas, e =>
        {
            Assert.NotNull(e.Colaborador?.Cargo);
            Assert.NotNull(e.Facilitador);
            Assert.NotNull(e.Unidade);
            Assert.NotNull(e.Motivo);
            Assert.NotEmpty(e.Itens);
            Assert.All(e.Itens, i => Assert.NotNull(i.Item?.Categoria));
        });
    }

    [FactPostgres]
    public async Task BuscarColaborador_IgnoraMaiusculasEBuscaPorDrt()
    {
        var servico = Provider.GetRequiredService<IColaboradorService>();

        Assert.Single(await servico.BuscarAsync("roberto carlos"));
        Assert.Single(await servico.BuscarAsync("10234"));
    }

    [FactPostgres]
    public async Task RegistrarEEstornarEntrega_PersisteNoBanco()
    {
        var entregaServico = Provider.GetRequiredService<IEntregaService>();
        var (roberto, facilitador, motivo) = await DadosDeEntregaAsync("Roberto Carlos");
        var capacete = (await Provider.GetRequiredService<ICatalogoService>().ListarItensAsync()).Single(i => i.Nome.Contains("Capacete"));

        var entrega = await entregaServico.RegistrarAsync(new RegistrarEntregaInput
        {
            ColaboradorId = roberto.Id,
            FacilitadorId = facilitador.Id,
            UnidadeId = roberto.UnidadeId,
            MotivoId = motivo.Id,
            TipoMovimentacao = TipoMovimentacao.Reposicao,
            Itens = [new RegistrarEntregaItemInput { ItemId = capacete.Id, Quantidade = 1 }]
        });

        await entregaServico.EstornarAsync(entrega.Id, facilitador.Id, "Teste de estorno");

        var historico = await entregaServico.ObterHistoricoPorColaboradorAsync(roberto.Id);
        var salva = historico.Single(e => e.Id == entrega.Id);
        Assert.Equal(StatusEntrega.Estornada, salva.Status);
        Assert.Contains("Teste de estorno", salva.Observacao);
        Assert.Equal(capacete.Id, Assert.Single(salva.Itens).ItemId);

        var logs = await Provider.GetRequiredService<IAuditoriaService>().ConsultarAsync(null, nameof(Entrega), null, null);
        Assert.Equal(2, logs.Count(l => l.EntidadeId == entrega.Id));
    }

    [FactPostgres]
    public async Task RegistrarEntrega_ForaDoPerfilDoCargo_NaoGravaNada()
    {
        var entregaServico = Provider.GetRequiredService<IEntregaService>();
        var (rafael, facilitador, motivo) = await DadosDeEntregaAsync("Rafael Costa");
        var cinto = (await Provider.GetRequiredService<ICatalogoService>().ListarItensAsync()).Single(i => i.Nome.Contains("Cinto"));
        var totalAntes = (await entregaServico.ConsultarAsync(new FiltroEntregas())).Count;

        await Assert.ThrowsAsync<ItemNaoElegivelException>(() => entregaServico.RegistrarAsync(new RegistrarEntregaInput
        {
            ColaboradorId = rafael.Id,
            FacilitadorId = facilitador.Id,
            UnidadeId = rafael.UnidadeId,
            MotivoId = motivo.Id,
            TipoMovimentacao = TipoMovimentacao.EntregaInicial,
            Itens = [new RegistrarEntregaItemInput { ItemId = cinto.Id, Quantidade = 1 }]
        }));

        Assert.Equal(totalAntes, (await entregaServico.ConsultarAsync(new FiltroEntregas())).Count);
    }

    [FactPostgres]
    public async Task DefinirPermissoes_SubstituiOPerfilDoCargo()
    {
        var elegibilidade = Provider.GetRequiredService<IElegibilidadeService>();
        var cargo = (await Provider.GetRequiredService<ICargoService>().ListarAsync()).Single(c => c.Nome == "Analista Administrativo");
        var oculos = (await Provider.GetRequiredService<ICatalogoService>().ListarItensAsync()).Single(i => i.Nome.Contains("Óculos"));
        var admin = (await Provider.GetRequiredService<IUsuarioService>().ListarAsync()).First(u => u.Perfil == PerfilUsuario.Administrador);

        await elegibilidade.DefinirPermissoesAsync(cargo.Id, [oculos.Id], admin.Id);
        Assert.Equal(oculos.Id, Assert.Single(await elegibilidade.ObterItensElegiveisPorCargoAsync(cargo.Id)).Id);

        await elegibilidade.DefinirPermissoesAsync(cargo.Id, [], admin.Id);
        Assert.Empty(await elegibilidade.ObterItensElegiveisPorCargoAsync(cargo.Id));
    }

    [FactPostgres]
    public async Task EditarItem_AtualizaCamposSemAlterarDataDeCriacao()
    {
        var catalogo = Provider.GetRequiredService<ICatalogoService>();
        var original = (await catalogo.ListarItensAsync()).Single(i => i.Codigo == "EPI-001");

        // Igual à tela de catálogo: monta um objeto novo com o mesmo Id (CriadoEm vem com "agora").
        await catalogo.SalvarItemAsync(new ItemEpiEpc
        {
            Id = original.Id,
            Codigo = original.Codigo,
            Nome = "Capacete Editado",
            CategoriaId = original.CategoriaId,
            NumeroCa = original.NumeroCa,
            ValidadePadraoMeses = 48,
            PossuiTamanho = original.PossuiTamanho
        });

        var editado = (await catalogo.ListarItensAsync()).Single(i => i.Id == original.Id);
        Assert.Equal("Capacete Editado", editado.Nome);
        Assert.Equal(48, editado.ValidadePadraoMeses);
        Assert.Equal(original.CriadoEm, editado.CriadoEm);
        Assert.NotNull(editado.Categoria);
    }

    [FactPostgres]
    public async Task Dashboard_CalculaIndicadoresAPartirDoBanco()
    {
        var indicadores = await Provider.GetRequiredService<IDashboardService>().ObterIndicadoresAsync();

        Assert.True(indicadores.TotalEntregas > 0);
        Assert.Equal(0, indicadores.TotalEstornos);
        Assert.Equal(2, indicadores.ColaboradoresAfastadosOuInativos);
        Assert.NotEmpty(indicadores.TopItens);
    }

    [FactPostgres]
    public async Task Solicitacoes_SeedEFluxoAteAEntrega_PersistemNoBanco()
    {
        var servico = Provider.GetRequiredService<ISolicitacaoService>();
        var roberto = (await Provider.GetRequiredService<IColaboradorService>().AutenticarAsync("10234", "123456"));
        var sst = (await Provider.GetRequiredService<IUsuarioService>().ListarAsync()).First(u => u.Perfil == PerfilUsuario.SegurancaTrabalho);

        Assert.Equal(32, (await servico.ConsultarAsync(new FiltroSolicitacoes())).Count);
        // Busca por nome do item, sem diferenciar maiúsculas (ILIKE).
        Assert.Single(await servico.ConsultarAsync(new FiltroSolicitacoes { Termo = "BOTA", ColaboradorId = roberto.Id }));

        var aprovada = (await servico.ConsultarAsync(new FiltroSolicitacoes { ColaboradorId = roberto.Id, Status = [StatusSolicitacao.Aprovada] })).Single();
        var entrega = await servico.RegistrarEntregaAsync(aprovada.Id, sst.Id);

        var final = (await servico.ObterAsync(aprovada.Id))!;
        Assert.Equal(StatusSolicitacao.Entregue, final.Status);
        Assert.Equal(entrega.Id, final.EntregaId);
        Assert.Equal(StatusSolicitacao.Entregue, final.Historico.Last().Status);
        Assert.NotNull(final.Colaborador?.Unidade);
        Assert.NotEmpty(final.Assinatura);

        var posse = await Provider.GetRequiredService<IColaboradorService>().ObterItensEmPosseAsync(roberto.Id);
        Assert.Equal(DateTime.Today, posse.Single(p => p.Item.Id == aprovada.ItemId).RecebidoEm.Date);
    }

    private async Task<(Colaborador Colaborador, Usuario Facilitador, MotivoMovimentacao Motivo)> DadosDeEntregaAsync(string nome)
    {
        var colaborador = (await Provider.GetRequiredService<IColaboradorService>().BuscarAsync(nome)).Single();
        var facilitador = (await Provider.GetRequiredService<IUsuarioService>().ListarAsync()).First(u => u.Perfil == PerfilUsuario.Facilitador);
        var motivo = (await Provider.GetRequiredService<IMotivoMovimentacaoRepository>().ListarAsync()).First();
        return (colaborador, facilitador, motivo);
    }
}
