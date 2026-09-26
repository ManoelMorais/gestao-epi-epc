using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Services;

public class DashboardService(IEntregaRepository entregasRepositorio, IColaboradorRepository colaboradoresRepositorio) : IDashboardService
{
    public async Task<IndicadoresDashboard> ObterIndicadoresAsync(DateOnly? de = null, DateOnly? ate = null)
    {
        var todas = await entregasRepositorio.ConsultarAsync(new FiltroEntregas
        {
            DataInicio = de,
            DataFim = ate
        });

        var colaboradores = await colaboradoresRepositorio.ListarAsync();

        var confirmadas = todas.Where(e => e.Status == StatusEntrega.Confirmada).ToList();
        var itensEntregues = confirmadas.SelectMany(e => e.Itens.Select(i => (Entrega: e, ItemEntregue: i))).ToList();

        var totalEpi = itensEntregues
            .Where(x => x.ItemEntregue.Item?.Categoria?.Tipo == TipoItem.Epi)
            .Sum(x => x.ItemEntregue.Quantidade);

        var totalEpc = itensEntregues
            .Where(x => x.ItemEntregue.Item?.Categoria?.Tipo == TipoItem.Epc)
            .Sum(x => x.ItemEntregue.Quantidade);

        var limiteTrintaDias = DateTime.Now.AddDays(-30);

        var topColaboradores = confirmadas
            .Where(e => e.Colaborador is not null)
            .GroupBy(e => e.Colaborador!.Nome)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        var topItens = itensEntregues
            .Where(x => x.ItemEntregue.Item is not null)
            .GroupBy(x => x.ItemEntregue.Item!.Nome)
            .Select(g => new ItemContagem(g.Key, g.Sum(x => x.ItemEntregue.Quantidade)))
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        var entregasPorSetor = confirmadas
            .Where(e => e.Colaborador is not null)
            .GroupBy(e => e.Colaborador!.Area)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .ToList();

        var entregasPorUnidade = confirmadas
            .Where(e => e.Unidade is not null)
            .GroupBy(e => e.Unidade!.Nome)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .ToList();

        var entregasPorFacilitador = confirmadas
            .Where(e => e.Facilitador is not null)
            .GroupBy(e => e.Facilitador!.Nome)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        var entregasPorMotivo = confirmadas
            .Where(e => e.Motivo is not null)
            .GroupBy(e => e.Motivo!.Descricao)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        var inicioSerie = de ?? DateOnly.FromDateTime(DateTime.Now.AddDays(-29));
        var fimSerie = ate ?? DateOnly.FromDateTime(DateTime.Now);
        var contagemPorDia = confirmadas
            .GroupBy(e => DateOnly.FromDateTime(e.DataHora))
            .ToDictionary(g => g.Key, g => g.Count());

        var evolucaoDiaria = new List<PontoSerieTemporal>();
        for (var dia = inicioSerie; dia <= fimSerie; dia = dia.AddDays(1))
            evolucaoDiaria.Add(new PontoSerieTemporal(dia, contagemPorDia.GetValueOrDefault(dia)));

        return new IndicadoresDashboard
        {
            TotalEntregas = confirmadas.Count,
            TotalEpiEntregues = totalEpi,
            TotalEpcMovimentados = totalEpc,
            TotalTrocas = confirmadas.Count(e => e.TipoMovimentacao == TipoMovimentacao.Troca),
            EntregasUltimos30Dias = confirmadas.Count(e => e.DataHora >= limiteTrintaDias),
            TotalEstornos = todas.Count(e => e.Status == StatusEntrega.Estornada),
            ColaboradoresAfastadosOuInativos = colaboradores.Count(c => c.Status != StatusColaborador.Ativo),
            TopColaboradores = topColaboradores,
            TopItens = topItens,
            EntregasPorSetor = entregasPorSetor,
            EntregasPorUnidade = entregasPorUnidade,
            EntregasPorFacilitador = entregasPorFacilitador,
            EntregasPorMotivo = entregasPorMotivo,
            EvolucaoDiaria = evolucaoDiaria
        };
    }
}
