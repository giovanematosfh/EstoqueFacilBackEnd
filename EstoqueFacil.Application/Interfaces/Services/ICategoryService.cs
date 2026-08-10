using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<PagedResultDto<CategoryDto>> GetAllAsync(string? search, int page, int pageSize);
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto);
        Task RemoveAsync(int id);
    }
}
