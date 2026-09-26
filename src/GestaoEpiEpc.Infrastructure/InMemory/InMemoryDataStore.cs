using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Infrastructure.Seed;

namespace GestaoEpiEpc.Infrastructure.InMemory;

/// <summary>
/// "Banco de dados" em memória, vivo enquanto o processo roda. É o único lugar que muda quando o
/// banco de verdade for conectado: as listas abaixo dão lugar a um DbContext do Entity Framework
/// Core, e os repositórios em InMemory/ dão lugar a repositórios com EF — as interfaces em
/// Application/Abstractions e todo o resto do sistema não mudam.
/// </summary>
public class InMemoryDataStore
{
    public List<Unidade> Unidades { get; } = new();
    public List<Cargo> Cargos { get; } = new();
    public List<Colaborador> Colaboradores { get; } = new();
    public List<CategoriaItem> CategoriasItem { get; } = new();
    public List<ItemEpiEpc> Itens { get; } = new();
    public List<CargoItemPermitido> CargoItemPermitidos { get; } = new();
    public List<MotivoMovimentacao> MotivosMovimentacao { get; } = new();
    public List<Usuario> Usuarios { get; } = new();
    public List<Entrega> Entregas { get; } = new();
    public List<LogAuditoria> LogsAuditoria { get; } = new();

    public InMemoryDataStore()
    {
        DataSeeder.Popular(this);
    }

    /// <summary>Resolve as propriedades de navegação de uma entrega a partir das listas do store
    /// (o que o EF Core fará sozinho, via Include, quando o banco for conectado).</summary>
    public void ResolverNavegacoes(Entrega entrega)
    {
        entrega.Colaborador ??= Colaboradores.FirstOrDefault(c => c.Id == entrega.ColaboradorId);
        if (entrega.Colaborador is not null)
            entrega.Colaborador.Cargo ??= Cargos.FirstOrDefault(c => c.Id == entrega.Colaborador.CargoId);

        entrega.Facilitador ??= Usuarios.FirstOrDefault(u => u.Id == entrega.FacilitadorId);
        entrega.Unidade ??= Unidades.FirstOrDefault(u => u.Id == entrega.UnidadeId);
        entrega.Motivo ??= MotivosMovimentacao.FirstOrDefault(m => m.Id == entrega.MotivoId);

        foreach (var item in entrega.Itens)
        {
            item.Item ??= Itens.FirstOrDefault(i => i.Id == item.ItemId);
            if (item.Item is not null)
                item.Item.Categoria ??= CategoriasItem.FirstOrDefault(c => c.Id == item.Item.CategoriaId);
        }
    }
}
