using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IPurchaseRequestRepository : IGenericRepository<PurchaseRequest>
    {
        Task<PurchaseRequest?> GetWithDetailsAsync(int id);
        Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize);
        Task<IEnumerable<PurchaseRequest>> GetByDateRangeAsync(int? branchId, DateOnly from, DateOnly to);
    }
}
