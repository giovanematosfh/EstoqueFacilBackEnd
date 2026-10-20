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
                .Include(pr => pr.Branch)
                .Include(pr => pr.Supplier)
                .Include(pr => pr.Sector)
                .Include(pr => pr.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(pr => pr.Id == id);
        }

        public async Task<bool> ExistsWithNumberAsync(int branchId, string number, int? ignoreId = null)
        {
            return await DbSet.AnyAsync(pr => pr.BranchId == branchId && pr.Number == number && (!ignoreId.HasValue || pr.Id != ignoreId.Value));
        }

        public async Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedAsync(int branchId, string? search, PurchaseRequestStatus? status, int? productId, int page, int pageSize)
        {
            var query = DbSet.Where(pr => pr.BranchId == branchId);

            if (status.HasValue)
            {
                query = query.Where(pr => pr.Status == status.Value);
            }

            if (productId.HasValue)
            {
                query = query.Where(pr => pr.Items.Any(i => i.ProductId == productId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(pr =>
                    EF.Functions.ILike(pr.Number, $"%{search}%") ||
                    (pr.Supplier != null && EF.Functions.ILike(pr.Supplier.Name, $"%{search}%")) ||
                    pr.Items.Any(i =>
                        EF.Functions.ILike(i.Product.Name, $"%{search}%") ||
                        EF.Functions.ILike(i.Product.Sku, $"%{search}%")));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(pr => pr.Branch)
                .Include(pr => pr.Supplier)
                .Include(pr => pr.Sector)
                .Include(pr => pr.Items).ThenInclude(i => i.Product)
                .OrderByDescending(pr => pr.RequestDate)
                .ThenByDescending(pr => pr.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsSplitQuery()
                .ToListAsync();

            return (items, totalCount);
        }

        // branchId null = todas as filiais. Um registro por item, para os relatórios por produto.
        public async Task<IEnumerable<PurchaseRequestItem>> GetItemsByDateRangeAsync(int? branchId, DateOnly from, DateOnly to)
        {
            var query = Context.Set<PurchaseRequestItem>()
                .Include(i => i.Product)
                .Include(i => i.PurchaseRequest).ThenInclude(pr => pr.Branch)
                .Include(i => i.PurchaseRequest).ThenInclude(pr => pr.Supplier)
                .Include(i => i.PurchaseRequest).ThenInclude(pr => pr.Sector)
                .Where(i => i.PurchaseRequest.RequestDate >= from && i.PurchaseRequest.RequestDate <= to);

            if (branchId.HasValue)
            {
                query = query.Where(i => i.PurchaseRequest.BranchId == branchId.Value);
            }

            return await query
                .OrderBy(i => i.Product.Name)
                .ThenBy(i => i.PurchaseRequest.Branch.Name)
                .ThenBy(i => i.PurchaseRequest.RequestDate)
                .ToListAsync();
        }
    }
}
