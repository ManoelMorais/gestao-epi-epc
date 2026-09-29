namespace GestaoEpiEpc.Application.Dtos;

public class IndicadoresDashboard
{
    public int TotalEntregas { get; init; }
    public int TotalEpiEntregues { get; init; }
    public int TotalEpcMovimentados { get; init; }
    public int TotalTrocas { get; init; }
    public int EntregasUltimos30Dias { get; init; }
    public int TotalEstornos { get; init; }
    public int ColaboradoresAfastadosOuInativos { get; init; }
    /// <summary>Pedidos do app aguardando análise (pendentes + em análise).</summary>
    public int SolicitacoesEmAberto { get; init; }
    public int SolicitacoesAguardandoRetirada { get; init; }

    public IReadOnlyList<ItemContagem> TopColaboradores { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<ItemContagem> TopItens { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<ItemContagem> EntregasPorSetor { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<ItemContagem> EntregasPorUnidade { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<ItemContagem> EntregasPorFacilitador { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<ItemContagem> EntregasPorMotivo { get; init; } = Array.Empty<ItemContagem>();
    public IReadOnlyList<PontoSerieTemporal> EvolucaoDiaria { get; init; } = Array.Empty<PontoSerieTemporal>();
}

public record ItemContagem(string Rotulo, int Quantidade);

public record PontoSerieTemporal(DateOnly Data, int Quantidade);
