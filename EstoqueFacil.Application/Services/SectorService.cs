using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class SectorService : ISectorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SectorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<SectorDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (sectors, totalCount) = await _unitOfWork.Sectors.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<SectorDto>
            {
                Items = sectors.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<SectorDto> GetByIdAsync(int id)
        {
            return MapToDto(await GetExistingAsync(id));
        }

        public async Task<SectorDto> CreateAsync(SaveSectorDto dto)
        {
            var name = dto.Name.Trim();
            if (await _unitOfWork.Sectors.ExistsWithNameAsync(name))
            {
                throw new BusinessException($"Já existe um setor com o nome '{name}'.");
            }

            var sector = new Sector
            {
                Name = name,
                ResponsibleName = dto.ResponsibleName,
                Active = dto.Active,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Sectors.Add(sector);
            await _unitOfWork.CommitAsync();

            return MapToDto(sector);
        }

        public async Task<SectorDto> UpdateAsync(int id, SaveSectorDto dto)
        {
            var sector = await GetExistingAsync(id);

            var name = dto.Name.Trim();
            if (await _unitOfWork.Sectors.ExistsWithNameAsync(name, id))
            {
                throw new BusinessException($"Já existe um setor com o nome '{name}'.");
            }

            sector.Name = name;
            sector.ResponsibleName = dto.ResponsibleName;
            sector.Active = dto.Active;

            await _unitOfWork.CommitAsync();
            return MapToDto(sector);
        }

        public async Task RemoveAsync(int id)
        {
            var sector = await GetExistingAsync(id);

            if (await _unitOfWork.Sectors.IsInUseAsync(id))
            {
                throw new BusinessException("Este setor já foi usado em solicitações de compra. Desative-o em vez de excluir.");
            }

            _unitOfWork.Sectors.Remove(sector);
            await _unitOfWork.CommitAsync();
        }

        private async Task<Sector> GetExistingAsync(int id)
        {
            var sector = await _unitOfWork.Sectors.GetByIdAsync(id);
            return sector ?? throw new NotFoundException($"Setor com id {id} não foi encontrado.");
        }

        private static SectorDto MapToDto(Sector sector)
        {
            return new SectorDto
            {
                Id = sector.Id,
                Name = sector.Name,
                ResponsibleName = sector.ResponsibleName,
                Active = sector.Active,
                CreatedAt = sector.CreatedAt
            };
        }
    }
}
