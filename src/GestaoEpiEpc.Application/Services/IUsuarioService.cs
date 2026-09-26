using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<Usuario>> ListarAsync();
    Task<Usuario> SalvarAsync(Usuario usuario);
    Task InativarAsync(Guid usuarioId);
}
