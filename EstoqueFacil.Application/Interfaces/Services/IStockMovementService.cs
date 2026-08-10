using EstoqueFacil.Application.Dtos;

namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface IStockMovementService
    {
        Task<PagedResultDto<StockMovementDto>> GetAllAsync(string? search, int page, int pageSize);
        Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(int productId);
        Task<IEnumerable<StockMovementDto>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<StockMovementDto> RegisterAsync(CreateStockMovementDto dto);
    }
}
