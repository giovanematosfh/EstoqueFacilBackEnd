using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null);
        Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    }
}
