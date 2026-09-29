using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Seguranca;
using GestaoEpiEpc.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Tests;

public class ColaboradorServiceTests
{
    [Fact]
    public async Task ItensEmPosse_UsaAUltimaMovimentacaoEOrdenaPorVencimento()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<IColaboradorService>();
        var roberto = (await provider.GetRequiredService<IColaboradorRepository>().ObterPorDrtAsync("10234"))!;

        var posse = await servico.ObterItensEmPosseAsync(roberto.Id);

        // Kit do Roberto no app: 9 itens diferentes.
        Assert.Equal(9, posse.Count);
        // A bota foi trocada há 385 dias e vale 12 meses: vencida, então vem primeiro.
        Assert.Equal("EPI-009", posse[0].Item.Codigo);
        Assert.True(posse[0].Vencido);
        // A luva isolante foi trocada duas vezes; vale a troca mais recente (1 unidade, há 16 dias).
        var luva = posse.Single(p => p.Item.Codigo == "EPI-003");
        Assert.Equal(1, luva.Quantidade);
        Assert.InRange((DateTime.Today - luva.RecebidoEm.Date).Days, 15, 17);
        // EPC não tem validade e fica no fim da lista.
        Assert.All(posse.TakeLast(2), p => Assert.Null(p.VenceEm));
    }

    [Fact]
    public async Task Autenticar_ComDrtESenhaDemo_Entra()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<IColaboradorService>();

        var colaborador = await servico.AutenticarAsync(" 10234 ", "123456");

        Assert.Equal("Roberto Carlos Nascimento", colaborador.Nome);
        Assert.Equal("roberto.nascimento@amperion.com.br", colaborador.Email);
    }

    [Fact]
    public async Task Autenticar_SenhaErradaOuColaboradorInativo_Recusa()
    {
        using var provider = Fixture.CriarProvider();
        var servico = provider.GetRequiredService<IColaboradorService>();

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.AutenticarAsync("10234", "errada"));
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.AutenticarAsync("99999", "123456"));
        // Larissa (12101) está inativa: senha certa, acesso bloqueado.
        var inativa = await Assert.ThrowsAsync<RegraDeNegocioException>(() => servico.AutenticarAsync("12101", "123456"));
        Assert.Contains("desativado", inativa.Message);
    }

    [Fact]
    public void HashSenha_NaoGuardaTextoPuroEVerificaCorretamente()
    {
        var hash = HashSenha.Gerar("sigme");

        Assert.DoesNotContain("sigme", hash);
        Assert.NotEqual(hash, HashSenha.Gerar("sigme")); // salt aleatório
        Assert.True(HashSenha.Verificar("sigme", hash));
        Assert.False(HashSenha.Verificar("Sigme", hash));
        Assert.False(HashSenha.Verificar("sigme", null));
    }
}
