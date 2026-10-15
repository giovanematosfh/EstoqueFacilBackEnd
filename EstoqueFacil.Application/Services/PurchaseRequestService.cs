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

        public async Task<PagedResultDto<PurchaseRequestDto>> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize)
        {
            await EnsureBranchExistsAsync(branchId);

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _unitOfWork.PurchaseRequests.GetPagedAsync(branchId, search, status, productId, page, pageSize);

            return new PagedResultDto<PurchaseRequestDto>
            {
                Items = items.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<PurchaseRequestDto>> GetByDateRangeAsync(int branchId, DateOnly from, DateOnly to)
        {
            await EnsureBranchExistsAsync(branchId);

            var items = await _unitOfWork.PurchaseRequests.GetByDateRangeAsync(branchId, from, to);
            return items.Select(MapToDto);
        }

        public async Task<PurchaseRequestDto> GetByIdAsync(int id)
        {
            var purchaseRequest = await GetExistingAsync(id);
            return MapToDto(purchaseRequest);
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

            var number = dto.Number.Trim();

            // A SC em aberto mais recente fica visível direto no nível de estoque.
            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(dto.ProductId, dto.BranchId);
            productStock.PurchaseRequestNumber = number;

            var purchaseRequest = new PurchaseRequest
            {
                ProductId = dto.ProductId,
                BranchId = dto.BranchId,
                Number = number,
                Quantity = dto.Quantity,
                RequestDate = dto.RequestDate,
                Status = PurchaseRequestStatus.Open,
                RequesterName = dto.RequesterName,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.PurchaseRequests.Add(purchaseRequest);
            await _unitOfWork.CommitAsync();

            purchaseRequest.Product = product;
            purchaseRequest.Branch = branch;
            return MapToDto(purchaseRequest);
        }

        public async Task<PurchaseRequestDto> ReceiveAsync(int id, ReceivePurchaseRequestDto dto)
        {
            var purchaseRequest = await GetExistingAsync(id);
            EnsureOpen(purchaseRequest);

            // O recebimento dá entrada no estoque, na mesma transação.
            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(purchaseRequest.ProductId, purchaseRequest.BranchId);
            productStock.Quantity += dto.Quantity;
            ClearOpenNumber(productStock, purchaseRequest);

            _unitOfWork.StockMovements.Add(new StockMovement
            {
                ProductId = purchaseRequest.ProductId,
                BranchId = purchaseRequest.BranchId,
                Type = MovementType.Inbound,
                Quantity = dto.Quantity,
                Reason = $"Recebimento SC nº {purchaseRequest.Number}",
                MovementDate = DateTime.UtcNow,
                RequesterName = purchaseRequest.RequesterName
            });

            purchaseRequest.Status = PurchaseRequestStatus.Received;
            purchaseRequest.ReceivedDate = dto.ReceivedDate;
            purchaseRequest.ReceivedQuantity = dto.Quantity;

            await _unitOfWork.CommitAsync();
            return MapToDto(purchaseRequest);
        }

        public async Task<PurchaseRequestDto> CancelAsync(int id)
        {
            var purchaseRequest = await GetExistingAsync(id);
            EnsureOpen(purchaseRequest);

            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(purchaseRequest.ProductId, purchaseRequest.BranchId);
            ClearOpenNumber(productStock, purchaseRequest);

            purchaseRequest.Status = PurchaseRequestStatus.Canceled;

            await _unitOfWork.CommitAsync();
            return MapToDto(purchaseRequest);
        }

        private async Task<PurchaseRequest> GetExistingAsync(int id)
        {
            var purchaseRequest = await _unitOfWork.PurchaseRequests.GetWithDetailsAsync(id);
            if (purchaseRequest == null)
            {
                throw new NotFoundException($"Solicitação de compra com id {id} não foi encontrada.");
            }

            return purchaseRequest;
        }

        private static void EnsureOpen(PurchaseRequest purchaseRequest)
        {
            if (purchaseRequest.Status != PurchaseRequestStatus.Open)
            {
                throw new BusinessException($"A SC nº {purchaseRequest.Number} não está mais em aberto.");
            }
        }

        private static void ClearOpenNumber(ProductStock productStock, PurchaseRequest purchaseRequest)
        {
            if (productStock.PurchaseRequestNumber == purchaseRequest.Number)
            {
                productStock.PurchaseRequestNumber = null;
            }
        }

        private async Task EnsureBranchExistsAsync(int branchId)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {branchId} não foi encontrada.");
            }
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
                BranchName = purchaseRequest.Branch?.Name ?? string.Empty,
                Number = purchaseRequest.Number,
                Quantity = purchaseRequest.Quantity,
                RequestDate = purchaseRequest.RequestDate,
                Status = purchaseRequest.Status.ToString(),
                ReceivedDate = purchaseRequest.ReceivedDate,
                ReceivedQuantity = purchaseRequest.ReceivedQuantity,
                RequesterName = purchaseRequest.RequesterName,
                Notes = purchaseRequest.Notes,
                CreatedAt = purchaseRequest.CreatedAt
            };
        }
    }
}
