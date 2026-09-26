using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Application.Services;

public interface IEntregaService
{
    /// <summary>Registra uma entrega, validando a elegibilidade de cada item para o cargo do colaborador.</summary>
    /// <exception cref="GestaoEpiEpc.Application.Exceptions.ItemNaoElegivelException" />
    Task<Entrega> RegistrarAsync(RegistrarEntregaInput input);

    /// <summary>Estorna uma entrega (nunca apaga fisicamente — cria um novo registro referenciando o original).</summary>
    Task EstornarAsync(Guid entregaId, Guid usuarioResponsavelId, string justificativa);

    Task<IReadOnlyList<Entrega>> ConsultarAsync(FiltroEntregas filtro);
    Task<IReadOnlyList<Entrega>> ObterHistoricoPorColaboradorAsync(Guid colaboradorId);
}
