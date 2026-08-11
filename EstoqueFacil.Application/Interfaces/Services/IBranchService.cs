using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IBranchService
    {
        Task<PagedResultDto<BranchDto>> GetAllAsync(string? search, int page, int pageSize);
        Task<BranchDto> GetByIdAsync(int id);
        Task<BranchDto> CreateAsync(CreateBranchDto dto);
        Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto);
        Task RemoveAsync(int id);
    }
}
