using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IBranchRepository : IGenericRepository<Branch>
    {
        Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null);
        Task<(IEnumerable<Branch> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    }
}
