using GestaoEpiEpc.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestaoEpiEpc.Tests;

public class ElegibilidadeServiceTests
{
    [Fact]
    public async Task ItemDoCatalogoLiberadoParaOCargo_DeveSerElegivel()
    {
        using var provider = Fixture.CriarProvider();
        var cargoServico = provider.GetRequiredService<ICargoService>();
        var catalogoServico = provider.GetRequiredService<ICatalogoService>();
        var elegibilidadeServico = provider.GetRequiredService<IElegibilidadeService>();

        var eletricista = (await cargoServico.ListarAsync()).Single(c => c.Nome == "Eletricista de Rede");
        var capacete = (await catalogoServico.ListarItensAsync()).Single(i => i.Nome.Contains("Capacete"));

        var elegivel = await elegibilidadeServico.ItemEhElegivelAsync(eletricista.Id, capacete.Id);

        Assert.True(elegivel);
    }

    [Fact]
    public async Task ItemNaoLiberadoParaOCargo_NaoDeveSerElegivel()
    {
        using var provider = Fixture.CriarProvider();
        var cargoServico = provider.GetRequiredService<ICargoService>();
        var catalogoServico = provider.GetRequiredService<ICatalogoService>();
        var elegibilidadeServico = provider.GetRequiredService<IElegibilidadeService>();

        // Analista Administrativo é um cargo de escritório: nenhum item é elegível por padrão.
        var analista = (await cargoServico.ListarAsync()).Single(c => c.Nome == "Analista Administrativo");
        var cinto = (await catalogoServico.ListarItensAsync()).Single(i => i.Nome.Contains("Cinto"));

        var elegivel = await elegibilidadeServico.ItemEhElegivelAsync(analista.Id, cinto.Id);

        Assert.False(elegivel);
    }

    [Fact]
    public async Task DefinirPermissoes_SubstituiConjuntoAnteriorPorCompleto()
    {
        using var provider = Fixture.CriarProvider();
        var cargoServico = provider.GetRequiredService<ICargoService>();
        var catalogoServico = provider.GetRequiredService<ICatalogoService>();
        var elegibilidadeServico = provider.GetRequiredService<IElegibilidadeService>();
        var usuarioServico = provider.GetRequiredService<IUsuarioService>();

        var analista = (await cargoServico.ListarAsync()).Single(c => c.Nome == "Analista Administrativo");
        var oculos = (await catalogoServico.ListarItensAsync()).Single(i => i.Nome.Contains("Óculos"));
        var admin = (await usuarioServico.ListarAsync()).Single(u => u.Perfil == GestaoEpiEpc.Domain.Enums.PerfilUsuario.Administrador);

        // Libera um item para o Analista...
        await elegibilidadeServico.DefinirPermissoesAsync(analista.Id, [oculos.Id], admin.Id);
        Assert.True(await elegibilidadeServico.ItemEhElegivelAsync(analista.Id, oculos.Id));

        // ...e depois remove tudo de novo — a segunda chamada substitui, não soma.
        await elegibilidadeServico.DefinirPermissoesAsync(analista.Id, [], admin.Id);
        Assert.False(await elegibilidadeServico.ItemEhElegivelAsync(analista.Id, oculos.Id));
    }
}
