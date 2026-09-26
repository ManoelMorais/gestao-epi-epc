using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Services;

public class EntregaService(
    IEntregaRepository entregasRepositorio,
    IColaboradorRepository colaboradoresRepositorio,
    IItemEpiEpcRepository itensRepositorio,
    IElegibilidadeService elegibilidadeServico,
    ILogAuditoriaRepository auditoriaRepositorio) : IEntregaService
{
    public async Task<Entrega> RegistrarAsync(RegistrarEntregaInput input)
    {
        var colaborador = await colaboradoresRepositorio.ObterPorIdAsync(input.ColaboradorId)
            ?? throw new InvalidOperationException("Colaborador não encontrado.");

        foreach (var itemInput in input.Itens)
        {
            var elegivel = await elegibilidadeServico.ItemEhElegivelAsync(colaborador.CargoId, itemInput.ItemId);
            if (!elegivel)
            {
                var item = await itensRepositorio.ObterPorIdAsync(itemInput.ItemId);
                var cargo = colaborador.Cargo;
                throw new ItemNaoElegivelException(item?.Nome ?? itemInput.ItemId.ToString(), cargo?.Nome ?? colaborador.CargoId.ToString());
            }
        }

        var entrega = new Entrega
        {
            ColaboradorId = input.ColaboradorId,
            FacilitadorId = input.FacilitadorId,
            UnidadeId = input.UnidadeId,
            MotivoId = input.MotivoId,
            TipoMovimentacao = input.TipoMovimentacao,
            EntregaOrigemId = input.EntregaOrigemId,
            AssinaturaUrl = input.AssinaturaUrl,
            Observacao = input.Observacao,
            Status = StatusEntrega.Confirmada
        };

        entrega.Itens = input.Itens.Select(i => new EntregaItem
        {
            EntregaId = entrega.Id,
            ItemId = i.ItemId,
            Quantidade = i.Quantidade,
            Tamanho = i.Tamanho,
            NumeroSerie = i.NumeroSerie,
            FotoUrl = i.FotoUrl
        }).ToList();

        await entregasRepositorio.AdicionarAsync(entrega);
        await auditoriaRepositorio.RegistrarAsync(
            input.FacilitadorId,
            entidade: nameof(Entrega),
            entidadeId: entrega.Id,
            acao: $"Entrega registrada ({input.TipoMovimentacao})");

        return entrega;
    }

    public async Task EstornarAsync(Guid entregaId, Guid usuarioResponsavelId, string justificativa)
    {
        var entrega = await entregasRepositorio.ObterPorIdAsync(entregaId)
            ?? throw new InvalidOperationException("Entrega não encontrada.");

        if (entrega.Status == StatusEntrega.Estornada)
            throw new InvalidOperationException("Esta entrega já foi estornada.");

        // A entrega nunca é apagada: apenas muda de status, preservando o registro original (RF25).
        entrega.Status = StatusEntrega.Estornada;
        entrega.Observacao = string.IsNullOrWhiteSpace(entrega.Observacao)
            ? $"Estornada: {justificativa}"
            : $"{entrega.Observacao} | Estornada: {justificativa}";
        entrega.AtualizadoEm = DateTime.Now;

        await entregasRepositorio.AtualizarAsync(entrega);
        await auditoriaRepositorio.RegistrarAsync(
            usuarioResponsavelId,
            entidade: nameof(Entrega),
            entidadeId: entrega.Id,
            acao: "Entrega estornada",
            dadosAntes: nameof(StatusEntrega.Confirmada),
            dadosDepois: nameof(StatusEntrega.Estornada));
    }

    public Task<IReadOnlyList<Entrega>> ConsultarAsync(FiltroEntregas filtro) =>
        entregasRepositorio.ConsultarAsync(filtro);

    public Task<IReadOnlyList<Entrega>> ObterHistoricoPorColaboradorAsync(Guid colaboradorId) =>
        entregasRepositorio.ListarPorColaboradorAsync(colaboradorId);
}
