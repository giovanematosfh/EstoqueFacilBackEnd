using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanDto>> GetAllAsync();
        Task<PlanDto> GetByIdAsync(int id);
        Task<PlanDto> CreateAsync(SavePlanDto dto);
        Task<PlanDto> UpdateAsync(int id, SavePlanDto dto);
        Task RemoveAsync(int id);
    }
}
