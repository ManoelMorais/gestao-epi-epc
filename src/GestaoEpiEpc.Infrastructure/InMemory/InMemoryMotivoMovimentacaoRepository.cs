using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryMotivoMovimentacaoRepository(InMemoryDataStore store) : IMotivoMovimentacaoRepository
{
    public Task<MotivoMovimentacao?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.MotivosMovimentacao.FirstOrDefault(m => m.Id == id));

    public Task<IReadOnlyList<MotivoMovimentacao>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<MotivoMovimentacao>>(store.MotivosMovimentacao.OrderBy(m => m.Descricao).ToList());

    public Task AdicionarAsync(MotivoMovimentacao entidade)
    {
        store.MotivosMovimentacao.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(MotivoMovimentacao entidade)
    {
        var index = store.MotivosMovimentacao.FindIndex(m => m.Id == entidade.Id);
        if (index >= 0) store.MotivosMovimentacao[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.MotivosMovimentacao.RemoveAll(m => m.Id == id);
        return Task.CompletedTask;
    }
}
