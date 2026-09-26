using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestaoEpiEpc.Tests;

public class EntregaServiceTests
{
    [Fact]
    public async Task RegistrarEntrega_ComItemElegivel_Salva()
    {
        using var provider = Fixture.CriarProvider();
        var entregaServico = provider.GetRequiredService<IEntregaService>();
        var colaboradorServico = provider.GetRequiredService<IColaboradorService>();
        var catalogoServico = provider.GetRequiredService<ICatalogoService>();
        var usuarioServico = provider.GetRequiredService<IUsuarioService>();
        var motivosRepositorio = provider.GetRequiredService<IMotivoMovimentacaoRepository>();

        var roberto = (await colaboradorServico.BuscarAsync("Roberto Carlos")).Single();
        var capacete = (await catalogoServico.ListarItensAsync()).Single(i => i.Nome.Contains("Capacete"));
        var facilitador = (await usuarioServico.ListarAsync()).First(u => u.Perfil == PerfilUsuario.Facilitador);
        var motivo = (await motivosRepositorio.ListarAsync()).Single(m => m.Descricao == "Desgaste natural");

        var totalAntes = (await entregaServico.ConsultarAsync(new FiltroEntregas())).Count;

        var entrega = await entregaServico.RegistrarAsync(new RegistrarEntregaInput
        {
            ColaboradorId = roberto.Id,
            FacilitadorId = facilitador.Id,
            UnidadeId = roberto.UnidadeId,
            MotivoId = motivo.Id,
            TipoMovimentacao = TipoMovimentacao.Reposicao,
            Itens = [new RegistrarEntregaItemInput { ItemId = capacete.Id, Quantidade = 1 }]
        });

        var totalDepois = (await entregaServico.ConsultarAsync(new FiltroEntregas())).Count;

        Assert.Equal(totalAntes + 1, totalDepois);
        Assert.Equal(StatusEntrega.Confirmada, entrega.Status);
    }

    [Fact]
    public async Task RegistrarEntrega_ComItemForaDoPerfilDoCargo_LancaExcecao()
    {
        using var provider = Fixture.CriarProvider();
        var entregaServico = provider.GetRequiredService<IEntregaService>();
        var colaboradorServico = provider.GetRequiredService<IColaboradorService>();
        var catalogoServico = provider.GetRequiredService<ICatalogoService>();
        var usuarioServico = provider.GetRequiredService<IUsuarioService>();
        var motivosRepositorio = provider.GetRequiredService<IMotivoMovimentacaoRepository>();

        // Rafael é Analista Administrativo: nenhum EPI/EPC de campo é elegível para ele.
        var rafael = (await colaboradorServico.BuscarAsync("Rafael Costa")).Single();
        var cinto = (await catalogoServico.ListarItensAsync()).Single(i => i.Nome.Contains("Cinto"));
        var facilitador = (await usuarioServico.ListarAsync()).First(u => u.Perfil == PerfilUsuario.Facilitador);
        var motivo = (await motivosRepositorio.ListarAsync()).Single(m => m.Descricao == "Novo colaborador");

        var input = new RegistrarEntregaInput
        {
            ColaboradorId = rafael.Id,
            FacilitadorId = facilitador.Id,
            UnidadeId = rafael.UnidadeId,
            MotivoId = motivo.Id,
            TipoMovimentacao = TipoMovimentacao.EntregaInicial,
            Itens = [new RegistrarEntregaItemInput { ItemId = cinto.Id, Quantidade = 1 }]
        };

        await Assert.ThrowsAsync<ItemNaoElegivelException>(() => entregaServico.RegistrarAsync(input));
    }

    [Fact]
    public async Task EstornarEntrega_MarcaComoEstornadaSemApagarORegistro()
    {
        using var provider = Fixture.CriarProvider();
        var entregaServico = provider.GetRequiredService<IEntregaService>();
        var usuarioServico = provider.GetRequiredService<IUsuarioService>();

        var admin = (await usuarioServico.ListarAsync()).Single(u => u.Perfil == PerfilUsuario.Administrador);
        var alguma = (await entregaServico.ConsultarAsync(new FiltroEntregas())).First(e => e.Status == StatusEntrega.Confirmada);

        await entregaServico.EstornarAsync(alguma.Id, admin.Id, "Registro duplicado por engano.");

        var todas = await entregaServico.ConsultarAsync(new FiltroEntregas());
        var estornada = todas.Single(e => e.Id == alguma.Id);

        Assert.Equal(StatusEntrega.Estornada, estornada.Status);
        Assert.Contains("Registro duplicado", estornada.Observacao);

        // Estornar de novo deve falhar — não existe "estornar o estorno".
        await Assert.ThrowsAsync<InvalidOperationException>(() => entregaServico.EstornarAsync(alguma.Id, admin.Id, "Segunda tentativa"));
    }
}
