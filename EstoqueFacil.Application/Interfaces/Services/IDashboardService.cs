using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
