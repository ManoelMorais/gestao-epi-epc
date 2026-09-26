using GestaoEpiEpc.Domain.Common;

namespace GestaoEpiEpc.Application.Abstractions;

/// <summary>
/// Contrato genérico de persistência. A implementação atual (Infrastructure) guarda tudo em
/// memória; quando o banco de dados for conectado, ela é trocada por uma implementação com
/// Entity Framework Core sem que Application ou Desktop precisem mudar.
/// </summary>
public interface IRepository<T> where T : EntidadeBase
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<T>> ListarAsync();
    Task AdicionarAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(Guid id);
}
