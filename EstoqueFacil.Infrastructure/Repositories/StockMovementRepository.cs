using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await DbSet.Include(m => m.Product)
                .Include(m => m.Branch)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<(IEnumerable<StockMovement> Items, int TotalCount)> GetPagedAsync(string? search, int? branchId, int page, int pageSize)
        {
            var query = DbSet
                .Include(m => m.Product).ThenInclude(p => p.ProductStocks)
                .Include(m => m.Branch)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(m => m.BranchId == branchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m =>
                    (m.Product != null && EF.Functions.ILike(m.Product.Name, $"%{search}%")) ||
                    (m.Reason != null && EF.Functions.ILike(m.Reason, $"%{search}%")));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.MovementDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime from, DateTime to, int? branchId)
        {
            var query = DbSet
                .Include(m => m.Product).ThenInclude(p => p.ProductStocks)
                .Include(m => m.Branch)
                .Where(m => m.MovementDate >= from && m.MovementDate <= to);

            if (branchId.HasValue)
            {
                query = query.Where(m => m.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
        {
            return await DbSet
                .Include(m => m.Product).ThenInclude(p => p.ProductStocks)
                .Include(m => m.Branch)
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }
    }
}
