using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class ColaboradorService(IColaboradorRepository colaboradoresRepositorio) : IColaboradorService
{
    public Task<IReadOnlyList<Colaborador>> ListarAsync() => colaboradoresRepositorio.ListarAsync();

    public Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo) => colaboradoresRepositorio.BuscarAsync(termo);

    public Task<Colaborador?> ObterPorIdAsync(Guid id) => colaboradoresRepositorio.ObterPorIdAsync(id);
}
