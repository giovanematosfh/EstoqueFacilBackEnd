using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IPurchaseRequestRepository : IGenericRepository<PurchaseRequest>
    {
        Task<PurchaseRequest?> GetWithDetailsAsync(int id);
        Task<bool> ExistsWithNumberAsync(int branchId, string number);
        Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize);
        Task<IEnumerable<PurchaseRequestItem>> GetItemsByDateRangeAsync(int? branchId, DateOnly from, DateOnly to);
    }
}
