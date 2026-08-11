using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BranchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<BranchDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (branches, totalCount) = await _unitOfWork.Branches.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<BranchDto>
            {
                Items = branches.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BranchDto> GetByIdAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {id} não foi encontrada.");
            }

            return MapToDto(branch);
        }

        public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
        {
            if (await _unitOfWork.Branches.ExistsWithNameAsync(dto.Name))
            {
                throw new BusinessException($"Já existe uma filial com o nome '{dto.Name}'.");
            }

            var branch = new Branch
            {
                Name = dto.Name,
                Address = dto.Address,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Branches.Add(branch);
            await _unitOfWork.CommitAsync();

            await _unitOfWork.ProductStocks.CreateForAllProductsAsync(branch.Id);
            await _unitOfWork.CommitAsync();

            return MapToDto(branch);
        }

        public async Task<BranchDto> UpdateAsync(int id, UpdateBranchDto dto)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {id} não foi encontrada.");
            }

            if (await _unitOfWork.Branches.ExistsWithNameAsync(dto.Name, id))
            {
                throw new BusinessException($"Já existe uma filial com o nome '{dto.Name}'.");
            }

            branch.Name = dto.Name;
            branch.Address = dto.Address;
            branch.Active = dto.Active;

            await _unitOfWork.CommitAsync();

            return MapToDto(branch);
        }

        public async Task RemoveAsync(int id)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                throw new NotFoundException($"Filial com id {id} não foi encontrada.");
            }

            _unitOfWork.Branches.Remove(branch);
            await _unitOfWork.CommitAsync();
        }

        private static BranchDto MapToDto(Branch branch)
        {
            return new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                Active = branch.Active,
                CreatedAt = branch.CreatedAt
            };
        }
    }
}
