using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface ISectorRepository : IGenericRepository<Sector>
    {
        Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null);
        Task<bool> IsInUseAsync(int id);
        Task<(IEnumerable<Sector> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    }
}
