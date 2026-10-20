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

        public async Task<IEnumerable<PurchaseRequestLineDto>> GetLinesByDateRangeAsync(int? branchId, DateOnly from, DateOnly to)
        {
            if (branchId.HasValue)
            {
                await EnsureBranchExistsAsync(branchId.Value);
            }

            var items = await _unitOfWork.PurchaseRequests.GetItemsByDateRangeAsync(branchId, from, to);
            return items.Select(MapToLineDto);
        }

        public async Task<PurchaseRequestDto> GetByIdAsync(int id)
        {
            var purchaseRequest = await GetExistingAsync(id);
            return MapToDto(purchaseRequest);
        }

        public async Task<PurchaseRequestDto> CreateAsync(CreatePurchaseRequestDto dto)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {dto.BranchId} não foi encontrada.");
            }

            var number = dto.Number.Trim();
            if (await _unitOfWork.PurchaseRequests.ExistsWithNumberAsync(dto.BranchId, number))
            {
                throw new BusinessException($"Já existe uma SC nº {number} nesta filial.");
            }

            var purchaseRequest = new PurchaseRequest
            {
                BranchId = dto.BranchId,
                Number = number,
                RequestDate = dto.RequestDate,
                Status = PurchaseRequestStatus.Open,
                RequesterName = dto.RequesterName,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new NotFoundException($"Produto com id {itemDto.ProductId} não foi encontrado.");
                }

                // A SC em aberto mais recente fica visível direto no nível de estoque.
                var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(itemDto.ProductId, dto.BranchId);
                productStock.PurchaseRequestNumber = number;

                purchaseRequest.Items.Add(new PurchaseRequestItem
                {
                    ProductId = itemDto.ProductId,
                    Product = product,
                    Quantity = itemDto.Quantity,
                    Status = PurchaseRequestStatus.Open
                });
            }

            _unitOfWork.PurchaseRequests.Add(purchaseRequest);
            await _unitOfWork.CommitAsync();

            purchaseRequest.Branch = branch;
            return MapToDto(purchaseRequest);
        }

        public async Task<PurchaseRequestDto> ReceiveAsync(int id, ReceivePurchaseRequestDto dto)
        {
            var purchaseRequest = await GetExistingAsync(id);
            EnsureOpen(purchaseRequest);

            foreach (var received in dto.Items.Where(i => i.Quantity > 0))
            {
                var item = purchaseRequest.Items.FirstOrDefault(i => i.Id == received.ItemId);
                if (item == null)
                {
                    throw new NotFoundException($"O item {received.ItemId} não pertence à SC nº {purchaseRequest.Number}.");
                }

                if (item.Status != PurchaseRequestStatus.Open)
                {
                    throw new BusinessException($"O item {item.Product.Name} da SC nº {purchaseRequest.Number} já foi recebido ou cancelado.");
                }

                // O recebimento dá entrada no estoque, na mesma transação.
                var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(item.ProductId, purchaseRequest.BranchId);
                productStock.Quantity += received.Quantity;

                _unitOfWork.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    BranchId = purchaseRequest.BranchId,
                    Type = MovementType.Inbound,
                    Quantity = received.Quantity,
                    Reason = $"Recebimento SC nº {purchaseRequest.Number}",
                    MovementDate = DateTime.UtcNow,
                    RequesterName = purchaseRequest.RequesterName
                });

                item.Status = PurchaseRequestStatus.Received;
                item.ReceivedDate = dto.ReceivedDate;
                item.ReceivedQuantity = received.Quantity;
                ClearOpenNumber(productStock, purchaseRequest);
            }

            purchaseRequest.RefreshStatus();
            await _unitOfWork.CommitAsync();
            return MapToDto(purchaseRequest);
        }

        // Cancela os itens ainda pendentes; os já recebidos continuam registrados.
        public async Task<PurchaseRequestDto> CancelAsync(int id)
        {
            var purchaseRequest = await GetExistingAsync(id);
            EnsureOpen(purchaseRequest);

            foreach (var item in purchaseRequest.Items.Where(i => i.Status == PurchaseRequestStatus.Open))
            {
                var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(item.ProductId, purchaseRequest.BranchId);
                ClearOpenNumber(productStock, purchaseRequest);
                item.Status = PurchaseRequestStatus.Canceled;
            }

            purchaseRequest.RefreshStatus();
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
                BranchId = purchaseRequest.BranchId,
                BranchName = purchaseRequest.Branch?.Name ?? string.Empty,
                Number = purchaseRequest.Number,
                RequestDate = purchaseRequest.RequestDate,
                Status = purchaseRequest.Status.ToString(),
                RequesterName = purchaseRequest.RequesterName,
                Notes = purchaseRequest.Notes,
                CreatedAt = purchaseRequest.CreatedAt,
                Items = purchaseRequest.Items
                    .OrderBy(i => i.Product?.Name)
                    .Select(i => new PurchaseRequestItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product?.Name ?? string.Empty,
                        Sku = i.Product?.Sku ?? string.Empty,
                        Quantity = i.Quantity,
                        Status = i.Status.ToString(),
                        ReceivedDate = i.ReceivedDate,
                        ReceivedQuantity = i.ReceivedQuantity
                    })
                    .ToList()
            };
        }

        private static PurchaseRequestLineDto MapToLineDto(PurchaseRequestItem item)
        {
            var purchaseRequest = item.PurchaseRequest;
            return new PurchaseRequestLineDto
            {
                Id = item.Id,
                PurchaseRequestId = purchaseRequest.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Sku = item.Product?.Sku ?? string.Empty,
                BranchId = purchaseRequest.BranchId,
                BranchName = purchaseRequest.Branch?.Name ?? string.Empty,
                Number = purchaseRequest.Number,
                Quantity = item.Quantity,
                RequestDate = purchaseRequest.RequestDate,
                Status = item.Status.ToString(),
                ReceivedDate = item.ReceivedDate,
                ReceivedQuantity = item.ReceivedQuantity,
                RequesterName = purchaseRequest.RequesterName,
                Notes = purchaseRequest.Notes,
                CreatedAt = purchaseRequest.CreatedAt
            };
        }
    }
}
