using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<CategoryDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (categories, totalCount) = await _unitOfWork.Categories.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<CategoryDto>
            {
                Items = categories.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Categoria com id {id} não foi encontrada.");
            }

            return MapToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (await _unitOfWork.Categories.ExistsWithNameAsync(dto.Name))
            {
                throw new BusinessException($"Já existe uma categoria com o nome '{dto.Name}'.");
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Categories.Add(category);
            await _unitOfWork.CommitAsync();

            return MapToDto(category);
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Categoria com id {id} não foi encontrada.");
            }

            if (await _unitOfWork.Categories.ExistsWithNameAsync(dto.Name, id))
            {
                throw new BusinessException($"Já existe uma categoria com o nome '{dto.Name}'.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _unitOfWork.CommitAsync();

            return MapToDto(category);
        }

        public async Task RemoveAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Categoria com id {id} não foi encontrada.");
            }

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CommitAsync();
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                ProductCount = category.Products?.Count ?? 0
            };
        }
    }
}
