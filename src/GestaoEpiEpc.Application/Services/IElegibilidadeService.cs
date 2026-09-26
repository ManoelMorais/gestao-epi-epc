using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

/// <summary>
/// Regra central do sistema: cada colaborador só pode receber EPI/EPC compatível com o seu cargo.
/// Esta é a única porta de entrada para consultar ou alterar essa elegibilidade — tanto o desktop
/// (que a configura) quanto o futuro app mobile (que a consulta ao registrar uma entrega) passam por aqui.
/// </summary>
public interface IElegibilidadeService
{
    Task<IReadOnlyList<ItemEpiEpc>> ObterItensElegiveisPorCargoAsync(Guid cargoId);
    Task<IReadOnlyList<ItemEpiEpc>> ObterItensElegiveisPorColaboradorAsync(Guid colaboradorId);
    Task<bool> ItemEhElegivelAsync(Guid cargoId, Guid itemId);

    /// <summary>Substitui o conjunto de itens permitidos para o cargo pelo informado.</summary>
    Task DefinirPermissoesAsync(Guid cargoId, IReadOnlyCollection<Guid> itemIds, Guid usuarioResponsavelId);
}
