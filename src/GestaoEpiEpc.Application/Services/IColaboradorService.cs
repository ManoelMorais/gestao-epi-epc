using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

/// <summary>
/// Somente consulta: por decisão de negócio (RF05), a base de colaboradores vem de uma fonte
/// oficial (RH/AD) — este sistema não cadastra colaborador manualmente.
/// </summary>
public interface IColaboradorService
{
    Task<IReadOnlyList<Colaborador>> ListarAsync();
    Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo);
    Task<Colaborador?> ObterPorIdAsync(Guid id);
}
