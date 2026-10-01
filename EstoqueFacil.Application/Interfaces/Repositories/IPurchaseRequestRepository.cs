using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IPurchaseRequestRepository : IGenericRepository<PurchaseRequest>
    {
        Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize);
        Task<PurchaseRequest?> GetByIdWithProductAsync(int id);
    }
}
