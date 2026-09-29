using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Dtos;

/// <summary>Um EPI/EPC que está com o colaborador, com a validade calculada a partir da última movimentação.</summary>
public record ItemEmPosse(ItemEpiEpc Item, int Quantidade, string? Tamanho, DateTime RecebidoEm, DateTime? VenceEm, int? DiasParaVencer)
{
    public bool Vencido => DiasParaVencer < 0;
    public bool VenceEmBreve => DiasParaVencer is >= 0 and <= 30;
}
