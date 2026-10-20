using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface ISectorService
    {
        Task<PagedResultDto<SectorDto>> GetAllAsync(string? search, int page, int pageSize);
        Task<SectorDto> GetByIdAsync(int id);
        Task<SectorDto> CreateAsync(SaveSectorDto dto);
        Task<SectorDto> UpdateAsync(int id, SaveSectorDto dto);
        Task RemoveAsync(int id);
    }
}
