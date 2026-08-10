using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null)
        {
            return await DbSet.AnyAsync(c => c.Name == name && (!ignoreId.HasValue || c.Id != ignoreId.Value));
        }

        public async Task<(IEnumerable<Category> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => EF.Functions.ILike(c.Name, $"%{search}%"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
