using GestaoEpiEpc.Application.Dtos;

namespace GestaoEpiEpc.Application.Services;

public interface IDashboardService
{
    Task<IndicadoresDashboard> ObterIndicadoresAsync(DateOnly? de = null, DateOnly? ate = null);
}
