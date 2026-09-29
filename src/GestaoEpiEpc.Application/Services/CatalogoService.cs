using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public class CatalogoService(
    ICategoriaItemRepository categoriasRepositorio,
    IItemEpiEpcRepository itensRepositorio) : ICatalogoService
{
    public Task<IReadOnlyList<CategoriaItem>> ListarCategoriasAsync() =>
        categoriasRepositorio.ListarAsync();

    public async Task<CategoriaItem> SalvarCategoriaAsync(CategoriaItem categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Codigo))
            categoria.Codigo = "cat-" + Slug(categoria.Nome);

        if (await categoriasRepositorio.ObterPorIdAsync(categoria.Id) is null)
            await categoriasRepositorio.AdicionarAsync(categoria);
        else
            await categoriasRepositorio.AtualizarAsync(categoria);

        return categoria;
    }

    public Task<IReadOnlyList<ItemEpiEpc>> ListarItensAsync() =>
        itensRepositorio.ListarAsync();

    public Task<IReadOnlyList<ItemEpiEpc>> BuscarItensAsync(string termo) =>
        itensRepositorio.BuscarAsync(termo);

    public async Task<ItemEpiEpc> SalvarItemAsync(ItemEpiEpc item)
    {
        if (await itensRepositorio.ObterPorIdAsync(item.Id) is null)
            await itensRepositorio.AdicionarAsync(item);
        else
            await itensRepositorio.AtualizarAsync(item);

        return item;
    }

    /// <summary>"Proteção dos Pés" → "protecao-dos-pes".</summary>
    private static string Slug(string texto)
    {
        var semAcento = new string(texto.Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray()).ToLowerInvariant();
        return System.Text.RegularExpressions.Regex.Replace(semAcento, "[^a-z0-9]+", "-").Trim('-');
    }

    public async Task InativarItemAsync(Guid itemId)
    {
        var item = await itensRepositorio.ObterPorIdAsync(itemId)
            ?? throw new InvalidOperationException("Item não encontrado.");

        item.Ativo = false;
        item.AtualizadoEm = DateTime.Now;
        await itensRepositorio.AtualizarAsync(item);
    }
}
