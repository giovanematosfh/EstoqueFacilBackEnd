using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlanLimitService _planLimits;
        private readonly IFileStorage _fileStorage;

        public const string ImageFolder = "products";

        public ProductService(IUnitOfWork unitOfWork, IPlanLimitService planLimits, IFileStorage fileStorage)
        {
            _unitOfWork = unitOfWork;
            _planLimits = planLimits;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResultDto<ProductDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<ProductDto>
            {
                Items = products.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {id} não foi encontrado.");
            }

            return MapToDto(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            await _planLimits.EnsureCanAddProductAsync();

            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Categoria com id {dto.CategoryId} não foi encontrada.");
            }

            if (await _unitOfWork.Products.ExistsWithSkuAsync(dto.Sku))
            {
                throw new BusinessException($"Já existe um produto com o SKU '{dto.Sku}'.");
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Sku = dto.Sku,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Products.Add(product);
            await _unitOfWork.CommitAsync();

            await _unitOfWork.ProductStocks.CreateForAllBranchesAsync(product.Id);
            await _unitOfWork.CommitAsync();

            product = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
            return MapToDto(product!);
        }

        public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {id} não foi encontrado.");
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Categoria com id {dto.CategoryId} não foi encontrada.");
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Active = dto.Active;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();

            product = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
            return MapToDto(product!);
        }

        public async Task RemoveAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Produto com id {id} não foi encontrado.");
            }

            var imageFileName = product.ImageFileName;

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.CommitAsync();

            if (imageFileName != null)
            {
                _fileStorage.Delete(ImageFolder, imageFileName);
            }
        }

        public async Task<ProductDto> SetImageAsync(int id, Stream content, string extension)
        {
            var product = await GetWithCategoryOrThrowAsync(id);
            var previous = product.ImageFileName;

            product.ImageFileName = await _fileStorage.SaveAsync(ImageFolder, content, extension);
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CommitAsync();

            if (previous != null)
            {
                _fileStorage.Delete(ImageFolder, previous);
            }

            return MapToDto(product);
        }

        public async Task<ProductDto> RemoveImageAsync(int id)
        {
            var product = await GetWithCategoryOrThrowAsync(id);
            if (product.ImageFileName == null)
            {
                return MapToDto(product);
            }

            var previous = product.ImageFileName;
            product.ImageFileName = null;
            product.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.CommitAsync();

            _fileStorage.Delete(ImageFolder, previous);
            return MapToDto(product);
        }

        private async Task<Product> GetWithCategoryOrThrowAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
            return product ?? throw new NotFoundException($"Produto com id {id} não foi encontrado.");
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Sku = product.Sku,
                Price = product.Price,
                Active = product.Active,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                ImageUrl = product.ImageFileName == null ? null : $"uploads/{ImageFolder}/{product.ImageFileName}"
            };
        }
    }
}
