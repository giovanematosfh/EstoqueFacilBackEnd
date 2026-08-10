using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface ISupplierService
    {
        Task<PagedResultDto<SupplierDto>> GetAllAsync(string? search, int page, int pageSize);
        Task<SupplierDto> GetByIdAsync(int id);
        Task<SupplierDto> CreateAsync(CreateSupplierDto dto);
        Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto dto);
        Task RemoveAsync(int id);
    }
}
