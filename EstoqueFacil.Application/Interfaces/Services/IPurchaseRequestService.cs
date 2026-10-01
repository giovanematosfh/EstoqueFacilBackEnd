using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IPurchaseRequestService
    {
        Task<PagedResultDto<PurchaseRequestDto>> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize);
        Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto);
        Task<PurchaseRequestDto> UpdateAsync(int id, UpdatePurchaseRequestDto dto);
        Task RemoveAsync(int id);
    }
}
