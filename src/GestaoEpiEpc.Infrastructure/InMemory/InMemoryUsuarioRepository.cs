using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Infrastructure.InMemory;

public class InMemoryUsuarioRepository(InMemoryDataStore store) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id) =>
        Task.FromResult(store.Usuarios.FirstOrDefault(u => u.Id == id));

    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        Task.FromResult(store.Usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyList<Usuario>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<Usuario>>(store.Usuarios.OrderBy(u => u.Nome).ToList());

    public Task AdicionarAsync(Usuario entidade)
    {
        store.Usuarios.Add(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Usuario entidade)
    {
        var index = store.Usuarios.FindIndex(u => u.Id == entidade.Id);
        if (index >= 0) store.Usuarios[index] = entidade;
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Guid id)
    {
        store.Usuarios.RemoveAll(u => u.Id == id);
        return Task.CompletedTask;
    }
}
