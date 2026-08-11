using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IProductStockService
    {
        Task<PagedResultDto<ProductStockDto>> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize);
        Task<IEnumerable<ProductStockDto>> GetLowStockAsync(int branchId);
        Task<IEnumerable<ProductStockDto>> GetAllByBranchAsync(int branchId);
        Task<ProductStockDto> UpdateMinimumAsync(int productId, int branchId, UpdateProductStockDto dto);
    }
}
