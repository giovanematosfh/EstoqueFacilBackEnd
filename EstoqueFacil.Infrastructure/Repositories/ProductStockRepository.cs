using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class ProductStockRepository : GenericRepository<ProductStock>, IProductStockRepository
    {
        public ProductStockRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ProductStock> GetOrCreateAsync(int productId, int branchId)
        {
            var existing = await DbSet
                .Include(ps => ps.Product)
                .Include(ps => ps.Branch)
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.BranchId == branchId);

            if (existing != null)
            {
                return existing;
            }

            var created = new ProductStock
            {
                ProductId = productId,
                BranchId = branchId,
                Quantity = 0,
                MinimumQuantity = 0
            };

            DbSet.Add(created);
            return created;
        }

        public async Task<(IEnumerable<ProductStock> Items, int TotalCount)> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize)
        {
            var query = DbSet.Include(ps => ps.Product).Where(ps => ps.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(ps =>
                    EF.Functions.ILike(ps.Product.Name, $"%{search}%") ||
                    EF.Functions.ILike(ps.Product.Sku, $"%{search}%"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(ps => ps.Product.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<ProductStock>> GetAllByBranchAsync(int branchId)
        {
            return await DbSet.Include(ps => ps.Product)
                .Where(ps => ps.BranchId == branchId)
                .OrderBy(ps => ps.Product.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductStock>> GetLowStockAsync(int branchId)
        {
            return await DbSet.Include(ps => ps.Product)
                .Where(ps => ps.BranchId == branchId && ps.Quantity <= ps.MinimumQuantity)
                .OrderBy(ps => ps.Product.Name)
                .ToListAsync();
        }

        public async Task CreateForAllBranchesAsync(int productId)
        {
            var branchIds = await Context.Set<Branch>().Where(b => b.Active).Select(b => b.Id).ToListAsync();
            var existingBranchIds = await DbSet.Where(ps => ps.ProductId == productId).Select(ps => ps.BranchId).ToListAsync();

            foreach (var branchId in branchIds.Except(existingBranchIds))
            {
                DbSet.Add(new ProductStock { ProductId = productId, BranchId = branchId, Quantity = 0, MinimumQuantity = 0 });
            }
        }

        public async Task CreateForAllProductsAsync(int branchId)
        {
            var productIds = await Context.Set<Product>().Select(p => p.Id).ToListAsync();
            var existingProductIds = await DbSet.Where(ps => ps.BranchId == branchId).Select(ps => ps.ProductId).ToListAsync();

            foreach (var productId in productIds.Except(existingProductIds))
            {
                DbSet.Add(new ProductStock { ProductId = productId, BranchId = branchId, Quantity = 0, MinimumQuantity = 0 });
            }
        }
    }
}
