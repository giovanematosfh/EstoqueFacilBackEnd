using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
    }
}
