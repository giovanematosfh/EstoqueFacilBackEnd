using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockMovementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<StockMovementDto>> GetAllAsync(string? search, int? branchId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (movements, totalCount) = await _unitOfWork.StockMovements.GetPagedAsync(search, branchId, page, pageSize);

            return new PagedResultDto<StockMovementDto>
            {
                Items = movements.Select(m => MapToDto(m, ResolveBalance(m))),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {productId} não foi encontrado.");
            }

            var movements = await _unitOfWork.StockMovements.GetByProductIdAsync(productId);
            return movements.Select(m => MapToDto(m, ResolveBalance(m)));
        }

        public async Task<IEnumerable<StockMovementDto>> GetByDateRangeAsync(DateTime from, DateTime to, int? branchId)
        {
            var movements = await _unitOfWork.StockMovements.GetByDateRangeAsync(from, to, branchId);
            return movements.Select(m => MapToDto(m, ResolveBalance(m)));
        }

        public async Task<StockMovementDto> RegisterAsync(CreateStockMovementDto dto)
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

            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(dto.ProductId, dto.BranchId);

            if (dto.Type == MovementType.Outbound && productStock.Quantity < dto.Quantity)
            {
                throw new BusinessException(
                    $"Estoque insuficiente para '{product.Name}' na filial '{branch.Name}'. Disponível: {productStock.Quantity}, solicitado: {dto.Quantity}.");
            }

            productStock.Quantity += dto.Type == MovementType.Inbound ? dto.Quantity : -dto.Quantity;

            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                BranchId = dto.BranchId,
                Type = dto.Type,
                Quantity = dto.Quantity,
                Reason = dto.Reason,
                MovementDate = DateTime.UtcNow,
                RequesterName = dto.RequesterName,
                Sector = dto.Sector
            };

            _unitOfWork.StockMovements.Add(movement);
            await _unitOfWork.CommitAsync();

            movement.Product = product;
            movement.Branch = branch;
            return MapToDto(movement, productStock.Quantity);
        }

        public async Task<StockMovementDto> UpdateAsync(int id, UpdateStockMovementDto dto)
        {
            var movement = await _unitOfWork.StockMovements.GetByIdAsync(id);
            if (movement == null)
            {
                throw new NotFoundException($"Movimentação com id {id} não foi encontrada.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(movement.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {movement.ProductId} não foi encontrado.");
            }

            var branch = await _unitOfWork.Branches.GetByIdAsync(movement.BranchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {movement.BranchId} não foi encontrada.");
            }

            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(movement.ProductId, movement.BranchId);

            var oldEffect = movement.Type == MovementType.Inbound ? movement.Quantity : -movement.Quantity;
            var newEffect = movement.Type == MovementType.Inbound ? dto.Quantity : -dto.Quantity;
            var newBalance = productStock.Quantity - oldEffect + newEffect;

            if (newBalance < 0)
            {
                throw new BusinessException(
                    $"Não é possível alterar a quantidade: o saldo de '{product.Name}' na filial '{branch.Name}' ficaria negativo.");
            }

            productStock.Quantity = newBalance;

            movement.Quantity = dto.Quantity;
            movement.Reason = dto.Reason;
            movement.RequesterName = dto.RequesterName;
            movement.Sector = dto.Sector;

            await _unitOfWork.CommitAsync();

            movement.Product = product;
            movement.Branch = branch;
            return MapToDto(movement, productStock.Quantity);
        }

        private static int ResolveBalance(StockMovement movement)
        {
            return movement.Product?.ProductStocks?.FirstOrDefault(ps => ps.BranchId == movement.BranchId)?.Quantity ?? 0;
        }

        private static StockMovementDto MapToDto(StockMovement movement, int currentBalance)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = movement.Product?.Name,
                BranchId = movement.BranchId,
                BranchName = movement.Branch?.Name ?? string.Empty,
                Type = movement.Type.ToString(),
                Quantity = movement.Quantity,
                Reason = movement.Reason,
                MovementDate = movement.MovementDate,
                StockBalanceAfter = currentBalance,
                RequesterName = movement.RequesterName,
                Sector = movement.Sector
            };
        }
    }
}
