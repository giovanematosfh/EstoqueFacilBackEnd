using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class ProductStockService : IProductStockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductStockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<ProductStockDto>> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize)
        {
            await EnsureBranchExistsAsync(branchId);

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _unitOfWork.ProductStocks.GetPagedByBranchAsync(branchId, search, page, pageSize);

            return new PagedResultDto<ProductStockDto>
            {
                Items = items.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<ProductStockDto>> GetLowStockAsync(int branchId)
        {
            await EnsureBranchExistsAsync(branchId);

            var items = await _unitOfWork.ProductStocks.GetLowStockAsync(branchId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductStockDto>> GetAllByBranchAsync(int branchId)
        {
            await EnsureBranchExistsAsync(branchId);

            var items = await _unitOfWork.ProductStocks.GetAllByBranchAsync(branchId);
            return items.Select(MapToDto);
        }

        public async Task<ProductStockDto> UpdateMinimumAsync(int productId, int branchId, UpdateProductStockDto dto)
        {
            await EnsureBranchExistsAsync(branchId);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {productId} não foi encontrado.");
            }

            var productStock = await _unitOfWork.ProductStocks.GetOrCreateAsync(productId, branchId);
            productStock.MinimumQuantity = dto.MinimumQuantity;

            await _unitOfWork.CommitAsync();

            if (productStock.Product == null)
            {
                productStock.Product = product;
            }

            return MapToDto(productStock);
        }

        private async Task EnsureBranchExistsAsync(int branchId)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {branchId} não foi encontrada.");
            }
        }

        private static ProductStockDto MapToDto(ProductStock productStock)
        {
            return new ProductStockDto
            {
                ProductId = productStock.ProductId,
                ProductName = productStock.Product?.Name ?? string.Empty,
                Sku = productStock.Product?.Sku ?? string.Empty,
                BranchId = productStock.BranchId,
                BranchName = productStock.Branch?.Name ?? string.Empty,
                Quantity = productStock.Quantity,
                MinimumQuantity = productStock.MinimumQuantity
            };
        }
    }
}
