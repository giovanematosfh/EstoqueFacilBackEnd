using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetByIdWithCategoryAsync(int id);
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<bool> ExistsWithSkuAsync(string sku, int? ignoreId = null);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    }
}
