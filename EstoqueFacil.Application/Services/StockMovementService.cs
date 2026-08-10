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

        public async Task<PagedResultDto<StockMovementDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (movements, totalCount) = await _unitOfWork.StockMovements.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<StockMovementDto>
            {
                Items = movements.Select(m => MapToDto(m, m.Product.StockQuantity)),
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
            return movements.Select(m => MapToDto(m, product.StockQuantity));
        }

        public async Task<IEnumerable<StockMovementDto>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            var movements = await _unitOfWork.StockMovements.GetByDateRangeAsync(from, to);
            return movements.Select(m => MapToDto(m, m.Product.StockQuantity));
        }

        public async Task<StockMovementDto> RegisterAsync(CreateStockMovementDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {dto.ProductId} não foi encontrado.");
            }

            if (dto.Type == MovementType.Outbound && product.StockQuantity < dto.Quantity)
            {
                throw new BusinessException(
                    $"Estoque insuficiente para '{product.Name}'. Disponível: {product.StockQuantity}, solicitado: {dto.Quantity}.");
            }

            product.StockQuantity += dto.Type == MovementType.Inbound ? dto.Quantity : -dto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                Type = dto.Type,
                Quantity = dto.Quantity,
                Reason = dto.Reason,
                MovementDate = DateTime.UtcNow
            };

            _unitOfWork.StockMovements.Add(movement);
            await _unitOfWork.CommitAsync();

            movement.Product = product;
            return MapToDto(movement, product.StockQuantity);
        }

        private static StockMovementDto MapToDto(StockMovement movement, int currentBalance)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = movement.Product?.Name,
                Type = movement.Type.ToString(),
                Quantity = movement.Quantity,
                Reason = movement.Reason,
                MovementDate = movement.MovementDate,
                StockBalanceAfter = currentBalance
            };
        }
    }
}
