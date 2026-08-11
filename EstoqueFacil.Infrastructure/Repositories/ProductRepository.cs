using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetByIdWithCategoryAsync(int id)
        {
            return await DbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
        {
            return await DbSet.Include(p => p.Category).ToListAsync();
        }

        public async Task<bool> ExistsWithSkuAsync(string sku, int? ignoreId = null)
        {
            return await DbSet.AnyAsync(p => p.Sku == sku && (!ignoreId.HasValue || p.Id != ignoreId.Value));
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = DbSet.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{search}%") ||
                    EF.Functions.ILike(p.Sku, $"%{search}%") ||
                    (p.Category != null && EF.Functions.ILike(p.Category.Name, $"%{search}%")));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
