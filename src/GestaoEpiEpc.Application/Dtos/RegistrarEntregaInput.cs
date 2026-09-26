using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Dtos;

/// <summary>Entrada para registrar uma entrega — hoje usada pelos testes e, futuramente, pelo app mobile.</summary>
public class RegistrarEntregaInput
{
    public required Guid ColaboradorId { get; init; }
    public required Guid FacilitadorId { get; init; }
    public required Guid UnidadeId { get; init; }
    public required Guid MotivoId { get; init; }
    public required TipoMovimentacao TipoMovimentacao { get; init; }
    public Guid? EntregaOrigemId { get; init; }
    public string? AssinaturaUrl { get; init; }
    public string? Observacao { get; init; }
    public required IReadOnlyList<RegistrarEntregaItemInput> Itens { get; init; }
}

public class RegistrarEntregaItemInput
{
    public required Guid ItemId { get; init; }
    public required int Quantidade { get; init; }
    public string? Tamanho { get; init; }
    public string? NumeroSerie { get; init; }
    public string? FotoUrl { get; init; }
}
