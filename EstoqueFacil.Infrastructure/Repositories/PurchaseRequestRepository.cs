using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class PurchaseRequestRepository : GenericRepository<PurchaseRequest>, IPurchaseRequestRepository
    {
        public PurchaseRequestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PurchaseRequest?> GetWithDetailsAsync(int id)
        {
            return await DbSet
                .Include(pr => pr.Product)
                .Include(pr => pr.Branch)
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }

        public async Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize)
        {
            var query = DbSet
                .Include(pr => pr.Product)
                .Include(pr => pr.Branch)
                .Where(pr => pr.BranchId == branchId);

            if (status.HasValue)
            {
                query = query.Where(pr => pr.Status == status.Value);
            }

            if (productId.HasValue)
            {
                query = query.Where(pr => pr.ProductId == productId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(pr =>
                    EF.Functions.ILike(pr.Number, $"%{search}%") ||
                    EF.Functions.ILike(pr.Product.Name, $"%{search}%") ||
                    EF.Functions.ILike(pr.Product.Sku, $"%{search}%"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(pr => pr.RequestDate)
                .ThenByDescending(pr => pr.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<PurchaseRequest>> GetByDateRangeAsync(int branchId, DateOnly from, DateOnly to)
        {
            return await DbSet
                .Include(pr => pr.Product)
                .Include(pr => pr.Branch)
                .Where(pr => pr.BranchId == branchId && pr.RequestDate >= from && pr.RequestDate <= to)
                .OrderBy(pr => pr.Product.Name)
                .ThenBy(pr => pr.RequestDate)
                .ToListAsync();
        }
    }
}
