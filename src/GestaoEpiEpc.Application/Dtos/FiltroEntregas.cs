using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Dtos;

/// <summary>Filtros combinados de consulta de entregas (RF17), usados na tela de acompanhamento.</summary>
public class FiltroEntregas
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public Guid? ColaboradorId { get; set; }
    public Guid? FacilitadorId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? CategoriaId { get; set; }
    public TipoMovimentacao? TipoMovimentacao { get; set; }
    public Guid? UnidadeId { get; set; }
    public string? Setor { get; set; }
}
