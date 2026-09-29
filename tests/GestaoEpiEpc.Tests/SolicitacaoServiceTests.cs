using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Tests;

public class SolicitacaoServiceTests
{
    [Fact]
    public async Task Seed_TemAsMesmasSolicitacoesDoApp()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var roberto = await ColaboradorAsync(provider, "10234");

        var todas = await servico.ConsultarAsync(new FiltroSolicitacoes());
        var doRoberto = await servico.ConsultarAsync(new FiltroSolicitacoes { ColaboradorId = roberto.Id });

        Assert.Equal(32, todas.Count);
        // Uma em cada etapa do fluxo (Entregue aparece duas vezes: luva e cone perdido).
        Assert.Equal(6, doRoberto.Count);
        Assert.Equal(
            [StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise, StatusSolicitacao.Aprovada, StatusSolicitacao.Entregue, StatusSolicitacao.Recusada],
            doRoberto.Select(s => s.Status).Distinct().Order());
        Assert.All(todas, s => Assert.Matches(@"^SOL-\d{4}-\d{5}$", s.Protocolo));
    }

    [Fact]
    public async Task FluxoCompleto_PendenteAteEntregue_GeraEntregaDeTroca()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var sst = await UsuarioAsync(provider, PerfilUsuario.SegurancaTrabalho);
        var bota = await SolicitacaoDoRobertoAsync(provider, StatusSolicitacao.Pendente);

        await servico.IniciarAnaliseAsync(bota.Id, sst.Id);
        await servico.AprovarAsync(bota.Id, sst.Id);
        var entrega = await servico.RegistrarEntregaAsync(bota.Id, sst.Id);

        var final = (await servico.ObterAsync(bota.Id))!;
        Assert.Equal(StatusSolicitacao.Entregue, final.Status);
        Assert.Equal(entrega.Id, final.EntregaId);
        Assert.Equal(
            [StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise, StatusSolicitacao.Aprovada, StatusSolicitacao.Entregue],
            final.Historico.Select(h => h.Status));
        Assert.Contains("(SST)", final.Historico.Single(h => h.Status == StatusSolicitacao.Aprovada).Responsavel);

        Assert.Equal(TipoMovimentacao.Troca, entrega.TipoMovimentacao);
        var itemEntregue = Assert.Single(entrega.Itens);
        Assert.Equal(bota.ItemId, itemEntregue.ItemId);
        Assert.Equal("42", itemEntregue.Tamanho);
    }

    [Fact]
    public async Task Recusar_SemMotivo_NaoPermite()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var sst = await UsuarioAsync(provider, PerfilUsuario.SegurancaTrabalho);
        var emAnalise = await SolicitacaoDoRobertoAsync(provider, StatusSolicitacao.EmAnalise);

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.RecusarAsync(emAnalise.Id, sst.Id, "  "));

        await servico.RecusarAsync(emAnalise.Id, sst.Id, "Lente ainda em bom estado.");
        var final = (await servico.ObterAsync(emAnalise.Id))!;
        Assert.Equal(StatusSolicitacao.Recusada, final.Status);
        Assert.Equal("Lente ainda em bom estado.", final.Historico.Last().Comentario);
    }

    [Fact]
    public async Task TransicaoInvalida_LancaRegraDeNegocio()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var sst = await UsuarioAsync(provider, PerfilUsuario.SegurancaTrabalho);
        var pendente = await SolicitacaoDoRobertoAsync(provider, StatusSolicitacao.Pendente);
        var recusada = await SolicitacaoDoRobertoAsync(provider, StatusSolicitacao.Recusada);

        // Não dá para entregar sem aprovar, nem reabrir uma recusada.
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.RegistrarEntregaAsync(pendente.Id, sst.Id));
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.AprovarAsync(recusada.Id, sst.Id));
    }

    [Fact]
    public async Task Criar_ValidaElegibilidadeDuplicidadeEGeraProtocoloSequencial()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var itens = await provider.GetRequiredService<ICatalogoService>().ListarItensAsync();
        var motivos = await provider.GetRequiredService<IMotivoMovimentacaoRepository>().ListarAsync();
        var desgaste = motivos.Single(m => m.Descricao == "Desgaste natural");
        var roberto = await ColaboradorAsync(provider, "10234");
        var rafael = await ColaboradorAsync(provider, "11345");

        NovaSolicitacaoInput Pedido(string codigo) => new()
        {
            ItemId = itens.Single(i => i.Codigo == codigo).Id,
            Quantidade = 1,
            Tamanho = "42",
            MotivoId = desgaste.Id,
            Relato = "Desgaste pelo uso.",
            Assinatura = "M0 0 L 10 10"
        };

        // Rafael é analista administrativo: nada é elegível.
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.CriarAsync(rafael.Id, Pedido("EPI-001")));
        // Roberto já tem a bota (EPI-009) pendente.
        var duplicada = await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.CriarAsync(roberto.Id, Pedido("EPI-009")));
        Assert.Contains("em andamento", duplicada.Message);

        // O capacete dele foi recusado antes — pode pedir de novo.
        var nova = await servico.CriarAsync(roberto.Id, Pedido("EPI-001"));

        Assert.Equal($"SOL-{DateTime.Now.Year}-02433", nova.Protocolo);
        Assert.Equal(StatusSolicitacao.Pendente, nova.Status);
        Assert.Null(nova.Tamanho); // capacete não tem tamanho
        Assert.Equal(roberto.Nome, Assert.Single(nova.Historico).Responsavel);
    }

    [Fact]
    public async Task Criar_MotivoDePerda_DescartaDadosDoMaterialAntigo()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<ISolicitacaoService>();
        var itens = await provider.GetRequiredService<ICatalogoService>().ListarItensAsync();
        var perda = (await provider.GetRequiredService<IMotivoMovimentacaoRepository>().ListarAsync()).Single(m => m.SemDevolucao);
        var juliana = await ColaboradorAsync(provider, "10567");

        var nova = await servico.CriarAsync(juliana.Id, new NovaSolicitacaoInput
        {
            ItemId = itens.Single(i => i.Codigo == "EPC-001").Id,
            Quantidade = 1,
            MotivoId = perda.Id,
            MaterialLote = "LT-1",
            MaterialMarca = "X",
            Relato = "Fita ficou no local do atendimento.",
            Assinatura = "M0 0 L 10 10"
        });

        Assert.Null(nova.MaterialLote);
        Assert.Null(nova.MaterialMarca);
    }

    private static async Task<Colaborador> ColaboradorAsync(IServiceProvider provider, string drt) =>
        (await provider.GetRequiredService<IColaboradorRepository>().ObterPorDrtAsync(drt))!;

    private static async Task<Usuario> UsuarioAsync(IServiceProvider provider, PerfilUsuario perfil) =>
        (await provider.GetRequiredService<IUsuarioService>().ListarAsync()).First(u => u.Perfil == perfil);

    private static async Task<Solicitacao> SolicitacaoDoRobertoAsync(IServiceProvider provider, StatusSolicitacao status)
    {
        var roberto = await ColaboradorAsync(provider, "10234");
        var lista = await provider.GetRequiredService<ISolicitacaoService>()
            .ConsultarAsync(new FiltroSolicitacoes { ColaboradorId = roberto.Id, Status = [status] });
        return lista.First();
    }
}
