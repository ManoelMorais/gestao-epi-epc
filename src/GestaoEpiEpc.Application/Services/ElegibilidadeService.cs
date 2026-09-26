using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class ElegibilidadeService(
    ICargoItemPermitidoRepository permissoesRepositorio,
    IItemEpiEpcRepository itensRepositorio,
    IColaboradorRepository colaboradoresRepositorio,
    ILogAuditoriaRepository auditoriaRepositorio) : IElegibilidadeService
{
    public async Task<IReadOnlyList<ItemEpiEpc>> ObterItensElegiveisPorCargoAsync(Guid cargoId)
    {
        var permissoes = await permissoesRepositorio.ListarPorCargoAsync(cargoId);
        if (permissoes.Count == 0)
            return Array.Empty<ItemEpiEpc>();

        return await itensRepositorio.ListarPorIdsAsync(permissoes.Select(p => p.ItemId));
    }

    public async Task<IReadOnlyList<ItemEpiEpc>> ObterItensElegiveisPorColaboradorAsync(Guid colaboradorId)
    {
        var colaborador = await colaboradoresRepositorio.ObterPorIdAsync(colaboradorId)
            ?? throw new InvalidOperationException("Colaborador não encontrado.");

        return await ObterItensElegiveisPorCargoAsync(colaborador.CargoId);
    }

    public async Task<bool> ItemEhElegivelAsync(Guid cargoId, Guid itemId)
    {
        var permissoes = await permissoesRepositorio.ListarPorCargoAsync(cargoId);
        return permissoes.Any(p => p.ItemId == itemId);
    }

    public async Task DefinirPermissoesAsync(Guid cargoId, IReadOnlyCollection<Guid> itemIds, Guid usuarioResponsavelId)
    {
        await permissoesRepositorio.RemoverPorCargoAsync(cargoId);

        foreach (var itemId in itemIds.Distinct())
        {
            await permissoesRepositorio.AdicionarAsync(new CargoItemPermitido
            {
                CargoId = cargoId,
                ItemId = itemId
            });
        }

        await auditoriaRepositorio.RegistrarAsync(
            usuarioResponsavelId,
            entidade: nameof(CargoItemPermitido),
            entidadeId: cargoId,
            acao: "Perfil de elegibilidade atualizado",
            dadosDepois: $"{itemIds.Count} item(ns) permitido(s)");
    }
}
