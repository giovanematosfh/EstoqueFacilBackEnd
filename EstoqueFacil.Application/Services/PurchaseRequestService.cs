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
            var purchaseRequest = new PurchaseRequest
            {
                Status = PurchaseRequestStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            await ApplyAsync(purchaseRequest, dto);

            _unitOfWork.PurchaseRequests.Add(purchaseRequest);
            await _unitOfWork.CommitAsync();

            return MapToDto(purchaseRequest);
        }

        // Só dá para editar enquanto nenhum item foi recebido ou cancelado.
        public async Task<PurchaseRequestDto> UpdateAsync(int id, CreatePurchaseRequestDto dto)
        {
            var purchaseRequest = await GetExistingAsync(id);
            if (purchaseRequest.Items.Any(i => i.Status != PurchaseRequestStatus.Open))
            {
                throw new BusinessException($"A SC nº {purchaseRequest.Number} já tem itens recebidos ou cancelados e não pode mais ser editada.");
            }

            if (dto.BranchId != purchaseRequest.BranchId)
            {
                throw new BusinessException("A filial de uma SC não pode ser alterada.");
            }

            // Tira o selo "SC nº" dos produtos antes de reaplicar com os itens novos.
            foreach (var item in purchaseRequest.Items)
            {
                var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(item.ProductId, purchaseRequest.BranchId);
                ClearOpenNumber(productStock, purchaseRequest);
            }

            await ApplyAsync(purchaseRequest, dto);
            await _unitOfWork.CommitAsync();

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
                    RequesterName = purchaseRequest.RequesterName,
                    Sector = purchaseRequest.Sector?.Name
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

        // Copia o cabeçalho e sincroniza os itens (mantém os que continuam, remove e adiciona o resto).
        private async Task ApplyAsync(PurchaseRequest purchaseRequest, CreatePurchaseRequestDto dto)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {dto.BranchId} não foi encontrada.");
            }

            var number = dto.Number.Trim();
            if (await _unitOfWork.PurchaseRequests.ExistsWithNumberAsync(dto.BranchId, number, purchaseRequest.Id == 0 ? null : purchaseRequest.Id))
            {
                throw new BusinessException($"Já existe uma SC nº {number} nesta filial.");
            }

            Supplier? supplier = null;
            if (dto.SupplierId.HasValue)
            {
                supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId.Value)
                    ?? throw new NotFoundException($"Fornecedor com id {dto.SupplierId} não foi encontrado.");
            }

            Sector? sector = null;
            if (dto.SectorId.HasValue)
            {
                sector = await _unitOfWork.Sectors.GetByIdAsync(dto.SectorId.Value)
                    ?? throw new NotFoundException($"Setor com id {dto.SectorId} não foi encontrado.");
            }

            purchaseRequest.BranchId = dto.BranchId;
            purchaseRequest.Branch = branch;
            purchaseRequest.Number = number;
            purchaseRequest.RequestDate = dto.RequestDate;
            purchaseRequest.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
            purchaseRequest.SupplierId = supplier?.Id;
            purchaseRequest.Supplier = supplier;
            purchaseRequest.SectorId = sector?.Id;
            purchaseRequest.Sector = sector;
            purchaseRequest.PaymentTerms = dto.PaymentTerms;
            purchaseRequest.RequesterName = dto.RequesterName;
            purchaseRequest.Notes = dto.Notes;

            var wanted = dto.Items.ToDictionary(i => i.ProductId);
            foreach (var removed in purchaseRequest.Items.Where(i => !wanted.ContainsKey(i.ProductId)).ToList())
            {
                purchaseRequest.Items.Remove(removed);
            }

            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId)
                    ?? throw new NotFoundException($"Produto com id {itemDto.ProductId} não foi encontrado.");

                var item = purchaseRequest.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);
                if (item == null)
                {
                    item = new PurchaseRequestItem { ProductId = itemDto.ProductId, Status = PurchaseRequestStatus.Open };
                    purchaseRequest.Items.Add(item);
                }

                item.Product = product;
                item.Quantity = itemDto.Quantity;
                item.UnitPrice = itemDto.UnitPrice;

                // A SC em aberto mais recente fica visível direto no nível de estoque.
                var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(itemDto.ProductId, dto.BranchId);
                productStock.PurchaseRequestNumber = number;
            }
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

        private static decimal? ItemTotal(PurchaseRequestItem item)
        {
            return item.UnitPrice.HasValue ? item.UnitPrice.Value * item.Quantity : null;
        }

        private static PurchaseRequestDto MapToDto(PurchaseRequest purchaseRequest)
        {
            var supplier = purchaseRequest.Supplier;
            return new PurchaseRequestDto
            {
                Id = purchaseRequest.Id,
                BranchId = purchaseRequest.BranchId,
                BranchName = purchaseRequest.Branch?.Name ?? string.Empty,
                Number = purchaseRequest.Number,
                RequestDate = purchaseRequest.RequestDate,
                ExpectedDeliveryDate = purchaseRequest.ExpectedDeliveryDate,
                SectorId = purchaseRequest.SectorId,
                SectorName = purchaseRequest.Sector?.Name,
                PaymentTerms = purchaseRequest.PaymentTerms,
                Supplier = supplier == null ? null : new PurchaseRequestSupplierDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    Document = supplier.Document,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address
                },
                TotalAmount = purchaseRequest.Items.Sum(i => ItemTotal(i) ?? 0),
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
                        UnitPrice = i.UnitPrice,
                        Total = ItemTotal(i),
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
                UnitPrice = item.UnitPrice,
                Total = ItemTotal(item),
                SupplierName = purchaseRequest.Supplier?.Name,
                SectorName = purchaseRequest.Sector?.Name,
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
