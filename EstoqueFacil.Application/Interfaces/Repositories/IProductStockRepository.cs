using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IProductStockRepository : IGenericRepository<ProductStock>
    {
        Task<ProductStock> GetOrCreateAsync(int productId, int branchId);
        Task<(IEnumerable<ProductStock> Items, int TotalCount)> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize);
        Task<IEnumerable<ProductStock>> GetAllByBranchAsync(int branchId);
        Task<IEnumerable<ProductStock>> GetLowStockAsync(int branchId);
        Task CreateForAllBranchesAsync(int productId);
        Task CreateForAllProductsAsync(int branchId);
    }
}
