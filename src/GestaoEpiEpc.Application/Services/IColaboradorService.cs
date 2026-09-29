using GestaoEpiEpc.Application.Dtos;
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

    /// <summary>O que está com o colaborador agora: a última movimentação confirmada de cada item, exceto devoluções.</summary>
    Task<IReadOnlyList<ItemEmPosse>> ObterItensEmPosseAsync(Guid colaboradorId);

    /// <summary>Login do app mobile por DRT e senha.</summary>
    /// <exception cref="GestaoEpiEpc.Application.Exceptions.RegraDeNegocioException">DRT/senha inválidos ou acesso desativado.</exception>
    Task<Colaborador> AutenticarAsync(string drt, string senha);
}
