using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryEntregaRepository(InMemoryDataStore store) : IEntregaRepository
{
    public Task<Entrega?> ObterPorIdAsync(Guid id)
    {
        var entrega = store.Entregas.FirstOrDefault(e => e.Id == id);
        if (entrega is not null) store.ResolverNavegacoes(entrega);
        return Task.FromResult(entrega);
    }

    public Task<IReadOnlyList<Entrega>> ListarAsync()
    {
        foreach (var e in store.Entregas) store.ResolverNavegacoes(e);
        return Task.FromResult<IReadOnlyList<Entrega>>(store.Entregas.OrderByDescending(e => e.DataHora).ToList());
    }

    public Task<IReadOnlyList<Entrega>> ConsultarAsync(FiltroEntregas filtro)
    {
        IEnumerable<Entrega> consulta = store.Entregas;

        if (filtro.DataInicio is { } dataInicio)
            consulta = consulta.Where(e => DateOnly.FromDateTime(e.DataHora) >= dataInicio);

        if (filtro.DataFim is { } dataFim)
            consulta = consulta.Where(e => DateOnly.FromDateTime(e.DataHora) <= dataFim);

        if (filtro.ColaboradorId is { } colaboradorId)
            consulta = consulta.Where(e => e.ColaboradorId == colaboradorId);

        if (filtro.FacilitadorId is { } facilitadorId)
            consulta = consulta.Where(e => e.FacilitadorId == facilitadorId);

        if (filtro.TipoMovimentacao is { } tipo)
            consulta = consulta.Where(e => e.TipoMovimentacao == tipo);

        if (filtro.UnidadeId is { } unidadeId)
            consulta = consulta.Where(e => e.UnidadeId == unidadeId);

        var resultado = consulta.OrderByDescending(e => e.DataHora).ToList();
        foreach (var e in resultado) store.ResolverNavegacoes(e);

        if (filtro.ItemId is { } itemId)
            resultado = resultado.Where(e => e.Itens.Any(i => i.ItemId == itemId)).ToList();

        if (filtro.CategoriaId is { } categoriaId)
            resultado = resultado.Where(e => e.Itens.Any(i => i.Item?.CategoriaId == categoriaId)).ToList();

        if (!string.IsNullOrWhiteSpace(filtro.Setor))
            resultado = resultado.Where(e => string.Equals(e.Colaborador?.Area, filtro.Setor, StringComparison.OrdinalIgnoreCase)).ToList();

        return Task.FromResult<IReadOnlyList<Entrega>>(resultado);
    }

    public Task<IReadOnlyList<Entrega>> ListarPorColaboradorAsync(Guid colaboradorId)
    {
        var resultado = store.Entregas
            .Where(e => e.ColaboradorId == colaboradorId)
            .OrderByDescending(e => e.DataHora)
            .ToList();

        foreach (var e in resultado) store.ResolverNavegacoes(e);
        return Task.FromResult<IReadOnlyList<Entrega>>(resultado);
    }

    public Task AdicionarAsync(Entrega entidade)
    {
        store.Entregas.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Entrega entidade)
    {
        var index = store.Entregas.FindIndex(e => e.Id == entidade.Id);
        if (index >= 0) store.Entregas[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Entregas.RemoveAll(e => e.Id == id);
        return Task.CompletedTask;
    }
}
