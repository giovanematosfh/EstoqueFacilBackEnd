using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IPurchaseRequestService
    {
        Task<PagedResultDto<PurchaseRequestDto>> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize);
        Task<IEnumerable<PurchaseRequestLineDto>> GetLinesByDateRangeAsync(int? branchId, DateOnly from, DateOnly to);
        Task<PurchaseRequestDto> GetByIdAsync(int id);
        Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto);
        Task<PurchaseRequestDto> ReceiveAsync(int id, ReceivePurchaseRequestDto dto);
        Task<PurchaseRequestDto> CancelAsync(int id);
    }
}
