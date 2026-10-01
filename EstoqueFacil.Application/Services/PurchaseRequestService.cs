using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class PurchaseRequestService : IPurchaseRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<PurchaseRequestDto>> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {branchId} não foi encontrada.");
            }

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _unitOfWork.PurchaseRequests.GetPagedByBranchAsync(branchId, search, page, pageSize);

            return new PagedResultDto<PurchaseRequestDto>
            {
                Items = items.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {dto.ProductId} não foi encontrado.");
            }

            var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {dto.BranchId} não foi encontrada.");
            }

            var purchaseRequest = new PurchaseRequest
            {
                ProductId = dto.ProductId,
                BranchId = dto.BranchId,
                RequestNumber = dto.RequestNumber,
                RequestDate = DateTime.SpecifyKind(dto.RequestDate, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.PurchaseRequests.Add(purchaseRequest);
            await _unitOfWork.CommitAsync();

            purchaseRequest.Product = product;
            return MapToDto(purchaseRequest);
        }

        public async Task<PurchaseRequestDto> UpdateAsync(int id, UpdatePurchaseRequestDto dto)
        {
            var purchaseRequest = await _unitOfWork.PurchaseRequests.GetByIdWithProductAsync(id);
            if (purchaseRequest == null)
            {
                throw new NotFoundException($"Solicitação de compra com id {id} não foi encontrada.");
            }

            purchaseRequest.RequestNumber = dto.RequestNumber;
            purchaseRequest.RequestDate = DateTime.SpecifyKind(dto.RequestDate, DateTimeKind.Utc);

            await _unitOfWork.CommitAsync();

            return MapToDto(purchaseRequest);
        }

        public async Task RemoveAsync(int id)
        {
            var purchaseRequest = await _unitOfWork.PurchaseRequests.GetByIdAsync(id);
            if (purchaseRequest == null)
            {
                throw new NotFoundException($"Solicitação de compra com id {id} não foi encontrada.");
            }

            _unitOfWork.PurchaseRequests.Remove(purchaseRequest);
            await _unitOfWork.CommitAsync();
        }

        private static PurchaseRequestDto MapToDto(PurchaseRequest purchaseRequest)
        {
            return new PurchaseRequestDto
            {
                Id = purchaseRequest.Id,
                ProductId = purchaseRequest.ProductId,
                ProductName = purchaseRequest.Product?.Name ?? string.Empty,
                Sku = purchaseRequest.Product?.Sku ?? string.Empty,
                BranchId = purchaseRequest.BranchId,
                RequestNumber = purchaseRequest.RequestNumber,
                RequestDate = purchaseRequest.RequestDate,
                CreatedAt = purchaseRequest.CreatedAt
            };
        }
    }
}
