using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class UsuarioService(IUsuarioRepository usuariosRepositorio) : IUsuarioService
{
    public Task<IReadOnlyList<Usuario>> ListarAsync() => usuariosRepositorio.ListarAsync();

    public async Task<Usuario> SalvarAsync(Usuario usuario)
    {
        if (await usuariosRepositorio.ObterPorIdAsync(usuario.Id) is null)
            await usuariosRepositorio.AdicionarAsync(usuario);
        else
            await usuariosRepositorio.AtualizarAsync(usuario);

        return usuario;
    }

    public async Task InativarAsync(Guid usuarioId)
    {
        var usuario = await usuariosRepositorio.ObterPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        usuario.Ativo = false;
        usuario.AtualizadoEm = DateTime.Now;
        await usuariosRepositorio.AtualizarAsync(usuario);
    }
}
