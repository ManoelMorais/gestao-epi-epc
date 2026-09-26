using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Abstractions;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email);
}
