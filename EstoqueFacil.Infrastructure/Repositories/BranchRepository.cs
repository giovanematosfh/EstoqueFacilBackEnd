using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class BranchRepository : GenericRepository<Branch>, IBranchRepository
    {
        public BranchRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null)
        {
            return await DbSet.AnyAsync(b => b.Name == name && (!ignoreId.HasValue || b.Id != ignoreId.Value));
        }

        public async Task<(IEnumerable<Branch> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => EF.Functions.ILike(b.Name, $"%{search}%"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
