using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IStockMovementRepository : IGenericRepository<StockMovement>
    {
        Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
        Task<(IEnumerable<StockMovement> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);
        Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
