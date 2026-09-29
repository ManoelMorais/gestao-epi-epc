namespace GestaoEpiEpc.Application.Dtos;

/// <summary>O que o app mobile envia ao pedir uma troca (mesmo formato de <c>NovaSolicitacaoInput</c> no app).</summary>
public class NovaSolicitacaoInput
{
    public required Guid ItemId { get; init; }
    public string? Tamanho { get; init; }
    public required int Quantidade { get; init; }
    public required Guid MotivoId { get; init; }
    public DateOnly? MaterialDataEntrega { get; init; }
    public string? MaterialFabricacao { get; init; }
    public string? MaterialMarca { get; init; }
    public string? MaterialLote { get; init; }
    public required string Relato { get; init; }
    public IReadOnlyList<string> Fotos { get; init; } = Array.Empty<string>();
    public required string Assinatura { get; init; }
}
