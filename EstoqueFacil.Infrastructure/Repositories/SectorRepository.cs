using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class SectorRepository : GenericRepository<Sector>, ISectorRepository
    {
        public SectorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsWithNameAsync(string name, int? ignoreId = null)
        {
            return await DbSet.AnyAsync(s => s.Name.ToLower() == name.ToLower() && (!ignoreId.HasValue || s.Id != ignoreId.Value));
        }

        public async Task<bool> IsInUseAsync(int id)
        {
            return await Context.Set<PurchaseRequest>().AnyAsync(pr => pr.SectorId == id);
        }

        public async Task<(IEnumerable<Sector> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    EF.Functions.ILike(s.Name, $"%{search}%") ||
                    (s.ResponsibleName != null && EF.Functions.ILike(s.ResponsibleName, $"%{search}%")));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
