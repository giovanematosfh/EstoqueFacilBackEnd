using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    EF.Functions.ILike(s.Name, $"%{search}%") ||
                    (s.Document != null && EF.Functions.ILike(s.Document, $"%{search}%")) ||
                    (s.Email != null && EF.Functions.ILike(s.Email, $"%{search}%")));
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
