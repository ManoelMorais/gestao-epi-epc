using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryColaboradorRepository(InMemoryDataStore store) : IColaboradorRepository
{
    public Task<Colaborador?> ObterPorIdAsync(Guid id)
    {
        var colaborador = store.Colaboradores.FirstOrDefault(c => c.Id == id);
        if (colaborador is not null)
            colaborador.Cargo ??= store.Cargos.FirstOrDefault(c => c.Id == colaborador.CargoId);

        return Task.FromResult(colaborador);
    }

    public Task<IReadOnlyList<Colaborador>> ListarAsync()
    {
        foreach (var c in store.Colaboradores)
            c.Cargo ??= store.Cargos.FirstOrDefault(cargo => cargo.Id == c.CargoId);

        return Task.FromResult<IReadOnlyList<Colaborador>>(store.Colaboradores.OrderBy(c => c.Nome).ToList());
    }

    public Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo)
    {
        termo = termo.Trim();
        var resultado = store.Colaboradores
            .Where(c => c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) || c.Matricula.Contains(termo, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Nome)
            .ToList();

        foreach (var c in resultado)
            c.Cargo ??= store.Cargos.FirstOrDefault(cargo => cargo.Id == c.CargoId);

        return Task.FromResult<IReadOnlyList<Colaborador>>(resultado);
    }

    public Task AdicionarAsync(Colaborador entidade)
    {
        store.Colaboradores.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Colaborador entidade)
    {
        var index = store.Colaboradores.FindIndex(c => c.Id == entidade.Id);
        if (index >= 0) store.Colaboradores[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Colaboradores.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
