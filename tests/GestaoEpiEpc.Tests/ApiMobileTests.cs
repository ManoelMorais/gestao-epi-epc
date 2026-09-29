using System.Text.Json;
using GestaoEpiEpc.Application;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Seguranca;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Enums;
using GestaoEpiEpc.Infrastructure;
using GestaoEpiEpc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GestaoEpiEpc.Tests;

/// <summary>
/// Testa as funções RPC que o app mobile chama no Supabase (Persistence/Sql/ApiMobile.sql) contra um
/// PostgreSQL real, e confere que dão o mesmo resultado que os serviços em C# usados pelo desktop.
/// </summary>
public class ApiMobileTests : IAsyncLifetime
{
    private ServiceProvider? _provider;
    private string _conexao = string.Empty;

    private ServiceProvider Provider => _provider!;

    public async Task InitializeAsync()
    {
        var baseConexao = Environment.GetEnvironmentVariable(FactPostgresAttribute.VariavelAmbiente);
        if (string.IsNullOrWhiteSpace(baseConexao)) return;

        _conexao = new NpgsqlConnectionStringBuilder(baseConexao) { Database = $"gestao_epi_api_{Guid.NewGuid():N}" }.ConnectionString;

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(_conexao);
        _provider = services.BuildServiceProvider();

        await Provider.GetRequiredService<InicializadorBancoDeDados>().InicializarAsync();
    }

    public async Task DisposeAsync()
    {
        if (_provider is null) return;

        NpgsqlConnection.ClearAllPools();
        await using (var db = await Provider.GetRequiredService<IDbContextFactory<GestaoEpiDbContext>>().CreateDbContextAsync())
            await db.Database.EnsureDeletedAsync();

        await _provider.DisposeAsync();
    }

    [FactPostgres]
    public async Task Login_DevolveTokenEColaboradorNoFormatoDoApp()
    {
        var sessao = await RpcAsync("app_login", ("p_drt", "10234"), ("p_senha", "123456"));

        Assert.Equal(64, sessao.GetProperty("token").GetString()!.Length);
        var colaborador = sessao.GetProperty("colaborador");
        Assert.Equal("Roberto Carlos Nascimento", colaborador.GetProperty("nome").GetString());
        Assert.Equal("2019-03-11", colaborador.GetProperty("admissao").GetString());
        Assert.Equal("Eletricista de Rede", colaborador.GetProperty("cargo").GetProperty("nome").GetString());
        Assert.Equal("Porto Sereno", colaborador.GetProperty("unidade").GetProperty("cidade").GetString());
    }

    [FactPostgres]
    public async Task Login_MensagensIguaisAsDoApp()
    {
        Assert.Equal("DRT ou senha inválidos.", await ErroAsync("app_login", ("p_drt", "10234"), ("p_senha", "errada")));
        Assert.Equal("DRT ou senha inválidos.", await ErroAsync("app_login", ("p_drt", "99999"), ("p_senha", "123456")));
        Assert.Equal("Seu acesso está desativado. Procure o RH ou o almoxarifado da sua unidade.",
            await ErroAsync("app_login", ("p_drt", "12101"), ("p_senha", "123456")));
        Assert.Equal("Sua sessão expirou. Entre novamente.", await ErroAsync("app_resumo", ("p_token", "token-inventado")));
    }

    [FactPostgres]
    public async Task ItensEmPosse_IguaisAoCalculoDoDesktop()
    {
        var token = await LoginAsync("10234");
        var roberto = (await Provider.GetRequiredService<IColaboradorRepository>().ObterPorDrtAsync("10234"))!;

        var doApp = (await RpcAsync("app_itens_em_posse", ("p_token", token))).EnumerateArray().ToList();
        var doDesktop = await Provider.GetRequiredService<IColaboradorService>().ObterItensEmPosseAsync(roberto.Id);

        Assert.Equal(doDesktop.Count, doApp.Count);
        Assert.Equal(
            doDesktop.Select(p => (p.Item.Id, p.Quantidade, p.DiasParaVencer)),
            doApp.Select(p => (
                p.GetProperty("item").GetProperty("id").GetGuid(),
                p.GetProperty("quantidade").GetInt32(),
                p.TryGetProperty("diasParaVencer", out var dias) ? dias.GetInt32() : (int?)null)));
        // Categoria sai com o código estável que o app usa (grade de tamanhos, ícones).
        Assert.Equal("cat-pes", doApp[0].GetProperty("item").GetProperty("categoriaId").GetString());
    }

    [FactPostgres]
    public async Task Resumo_ListaEFiltros_DoColaboradorLogado()
    {
        var token = await LoginAsync("10234");

        var resumo = await RpcAsync("app_resumo", ("p_token", token));
        Assert.Equal(6, resumo.GetProperty("totalSolicitacoes").GetInt32());
        Assert.Equal(2, resumo.GetProperty("porStatus").GetProperty("Entregue").GetInt32());
        Assert.Equal(3, resumo.GetProperty("recentes").GetArrayLength());

        Assert.Equal(6, (await RpcAsync("app_listar_solicitacoes", ("p_token", token))).GetArrayLength());
        Assert.Equal(2, (await RpcAsync("app_listar_solicitacoes", ("p_token", token), ("p_grupo", "andamento"))).GetArrayLength());
        // Busca sem acento/maiúscula: "OCULOS" acha "Óculos de Proteção Incolor".
        Assert.Equal(1, (await RpcAsync("app_listar_solicitacoes", ("p_token", token), ("p_termo", "OCULOS"))).GetArrayLength());

        var primeira = (await RpcAsync("app_listar_solicitacoes", ("p_token", token))).EnumerateArray().First();
        var historico = primeira.GetProperty("historico").EnumerateArray().First();
        Assert.Equal("Você", historico.GetProperty("responsavel").GetString());
        Assert.StartsWith("M20 80", primeira.GetProperty("assinatura").GetString());
    }

    [FactPostgres]
    public async Task ObterSolicitacao_DeOutroColaborador_VoltaNulo()
    {
        var tokenRoberto = await LoginAsync("10234");
        var tokenJuliana = await LoginAsync("10567");
        var daJuliana = (await RpcAsync("app_listar_solicitacoes", ("p_token", tokenJuliana))).EnumerateArray().First().GetProperty("id").GetGuid();

        Assert.Equal(JsonValueKind.Null, (await RpcAsync("app_obter_solicitacao", ("p_token", tokenRoberto), ("p_id", daJuliana))).ValueKind);
        Assert.Equal(daJuliana, (await RpcAsync("app_obter_solicitacao", ("p_token", tokenJuliana), ("p_id", daJuliana))).GetProperty("id").GetGuid());
    }

    [FactPostgres]
    public async Task Motivos_SoTrocaEReposicao_ComCodigosDoApp()
    {
        var motivos = (await RpcAsync("app_motivos")).EnumerateArray().Select(m => m.GetProperty("id").GetString()).ToList();

        Assert.Equal(6, motivos.Count);
        Assert.Contains("mot-validade", motivos);
        Assert.Equal("mot-perda", motivos[^1]);
        Assert.DoesNotContain("mot-novo", motivos);
    }

    [FactPostgres]
    public async Task CriarPeloApp_ValidaRegras_EDesktopAprovaEAppVeOResultado()
    {
        var tokenRoberto = await LoginAsync("10234");
        var tokenRafael = await LoginAsync("11345");
        var elegiveis = (await RpcAsync("app_itens_elegiveis", ("p_token", tokenRoberto))).EnumerateArray().ToList();
        Guid Item(string codigo) => elegiveis.Single(i => i.GetProperty("codigo").GetString() == codigo).GetProperty("id").GetGuid();

        object Pedido(Guid itemId, string? tamanho = null) => new
        {
            itemId,
            tamanho,
            quantidade = 1,
            motivoId = "mot-desgaste",
            materialAntigo = new { relato = "Aba trincada.", lote = "CP-1", dataEntrega = "2024-10-01T12:00:00.000Z" },
            fotos = Array.Empty<string>(),
            assinatura = "M0 0 L 10 10"
        };

        // Analista administrativo: nenhum item é elegível para o cargo dele.
        Assert.Equal("O item \"Capacete de Segurança Classe B\" não é elegível para o cargo Analista Administrativo.",
            await ErroAsync("app_criar_solicitacao", ("p_token", tokenRafael), ("p_input", Json(Pedido(Item("EPI-001"))))));
        // Roberto já tem a bota pendente.
        Assert.Matches(@"^Você já tem uma solicitação em andamento para este item \(SOL-\d{4}-02432\)\.$",
            await ErroAsync("app_criar_solicitacao", ("p_token", tokenRoberto), ("p_input", Json(Pedido(Item("EPI-009"), "42")))));

        // O capacete dele foi recusado antes: pode pedir de novo.
        var criada = await RpcAsync("app_criar_solicitacao", ("p_token", tokenRoberto), ("p_input", Json(Pedido(Item("EPI-001")))));
        Assert.Matches(@"^SOL-\d{4}-02433$", criada.GetProperty("protocolo").GetString());
        Assert.Equal("Pendente", criada.GetProperty("status").GetString());
        var id = criada.GetProperty("id").GetGuid();

        // O desktop enxerga o pedido e o SST aprova...
        var servico = Provider.GetRequiredService<ISolicitacaoService>();
        var noDesktop = (await servico.ObterAsync(id))!;
        Assert.Equal("Aba trincada.", noDesktop.Relato);
        Assert.Equal(new DateOnly(2024, 10, 1), noDesktop.MaterialDataEntrega);
        var sst = (await Provider.GetRequiredService<IUsuarioService>().ListarAsync()).First(u => u.Perfil == PerfilUsuario.SegurancaTrabalho);
        await servico.AprovarAsync(id, sst.Id);

        // ...e o app vê a aprovação, com a mensagem de retirada.
        var noApp = await RpcAsync("app_obter_solicitacao", ("p_token", tokenRoberto), ("p_id", id));
        Assert.Equal("Aprovada", noApp.GetProperty("status").GetString());
        var ultimo = noApp.GetProperty("historico").EnumerateArray().Last();
        Assert.Equal("Carlos Eduardo Lima (SST)", ultimo.GetProperty("responsavel").GetString());
        Assert.Equal("Retire o item no Almoxarifado APS.", ultimo.GetProperty("comentario").GetString());
    }

    [FactPostgres]
    public async Task Inicializador_RegravaHashLegadoDaSenhaDemoEmBcrypt()
    {
        await using (var db = await Provider.GetRequiredService<IDbContextFactory<GestaoEpiDbContext>>().CreateDbContextAsync())
        {
            var roberto = await db.Colaboradores.SingleAsync(c => c.Drt == "10234");
            roberto.SenhaHash = HashPbkdf2Legado("123456");
            await db.SaveChangesAsync();
        }
        Assert.Equal("DRT ou senha inválidos.", await ErroAsync("app_login", ("p_drt", "10234"), ("p_senha", "123456")));

        await Provider.GetRequiredService<InicializadorBancoDeDados>().InicializarAsync();

        Assert.NotEmpty(await LoginAsync("10234"));
    }

    // ---- Auxiliares ---------------------------------------------------------------------

    private async Task<string> LoginAsync(string drt) =>
        (await RpcAsync("app_login", ("p_drt", drt), ("p_senha", "123456"))).GetProperty("token").GetString()!;

    private static string Json(object valor) => JsonSerializer.Serialize(valor);

    /// <summary>Chama a função como o PostgREST faz (parâmetros nomeados) e devolve o JSON.</summary>
    private async Task<JsonElement> RpcAsync(string funcao, params (string Nome, object? Valor)[] parametros)
    {
        await using var conexao = new NpgsqlConnection(_conexao);
        await conexao.OpenAsync();

        var argumentos = string.Join(", ", parametros.Select((p, i) => $"{p.Nome} => @p{i}"));
        await using var comando = new NpgsqlCommand($"SELECT public.{funcao}({argumentos})::text", conexao);
        for (var i = 0; i < parametros.Length; i++)
        {
            var (nome, valor) = parametros[i];
            comando.Parameters.Add(nome == "p_input"
                ? new NpgsqlParameter($"p{i}", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = valor! }
                : new NpgsqlParameter($"p{i}", valor ?? DBNull.Value));
        }

        var resultado = await comando.ExecuteScalarAsync() as string;
        return JsonDocument.Parse(resultado ?? "null").RootElement.Clone();
    }

    private async Task<string> ErroAsync(string funcao, params (string Nome, object? Valor)[] parametros)
    {
        var erro = await Assert.ThrowsAsync<PostgresException>(() => RpcAsync(funcao, parametros));
        return erro.MessageText;
    }

    private static string HashPbkdf2Legado(string senha)
    {
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(senha, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        var legado = $"pbkdf2$100000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        Assert.True(HashSenha.EhLegado(legado) && HashSenha.Verificar(senha, legado));
        return legado;
    }
}
