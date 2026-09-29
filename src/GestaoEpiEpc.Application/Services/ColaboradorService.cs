using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Application.Seguranca;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Services;

public class ColaboradorService(
    IColaboradorRepository colaboradoresRepositorio,
    IEntregaRepository entregasRepositorio) : IColaboradorService
{
    public Task<IReadOnlyList<Colaborador>> ListarAsync() => colaboradoresRepositorio.ListarAsync();

    public Task<IReadOnlyList<Colaborador>> BuscarAsync(string termo) => colaboradoresRepositorio.BuscarAsync(termo);

    public Task<Colaborador?> ObterPorIdAsync(Guid id) => colaboradoresRepositorio.ObterPorIdAsync(id);

    public async Task<IReadOnlyList<ItemEmPosse>> ObterItensEmPosseAsync(Guid colaboradorId)
    {
        var entregas = await entregasRepositorio.ListarPorColaboradorAsync(colaboradorId);

        // A última movimentação confirmada de cada item define o que está com o colaborador (mesma regra do app).
        var ultimaPorItem = new Dictionary<Guid, (Entrega Entrega, EntregaItem Item)>();
        foreach (var entrega in entregas.Where(e => e.Status == StatusEntrega.Confirmada).OrderBy(e => e.DataHora))
            foreach (var item in entrega.Itens)
                ultimaPorItem[item.ItemId] = (entrega, item);

        var hoje = DateTime.Today;
        return ultimaPorItem.Values
            .Where(u => u.Entrega.TipoMovimentacao != TipoMovimentacao.Devolucao && u.Item.Item is not null)
            .Select(u =>
            {
                var validade = u.Item.Item!.ValidadePadraoMeses;
                DateTime? venceEm = validade is { } meses ? u.Entrega.DataHora.AddMonths(meses) : null;
                int? dias = venceEm is { } data ? (int)(data.Date - hoje).TotalDays : null;
                return new ItemEmPosse(u.Item.Item, u.Item.Quantidade, u.Item.Tamanho, u.Entrega.DataHora, venceEm, dias);
            })
            // Vencidos e próximos de vencer primeiro; itens sem validade no fim; empate pelo nome
            // (mesma ordem da função app_itens_em_posse que o app mobile usa).
            .OrderBy(p => p.DiasParaVencer ?? int.MaxValue)
            .ThenBy(p => p.Item.Nome, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<Colaborador> AutenticarAsync(string drt, string senha)
    {
        var colaborador = await colaboradoresRepositorio.ObterPorDrtAsync(drt.Trim());
        if (colaborador is null || !HashSenha.Verificar(senha, colaborador.SenhaHash))
            throw new RegraDeNegocioException("DRT ou senha inválidos.");

        if (colaborador.Status == StatusColaborador.Inativo)
            throw new RegraDeNegocioException("Seu acesso está desativado. Procure o RH ou o almoxarifado da sua unidade.");

        return colaborador;
    }
}
