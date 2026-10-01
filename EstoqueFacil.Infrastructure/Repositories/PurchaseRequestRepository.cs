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

        public async Task<(IEnumerable<PurchaseRequest> Items, int TotalCount)> GetPagedByBranchAsync(int branchId, string? search, int page, int pageSize)
        {
            var query = DbSet.Include(pr => pr.Product).Where(pr => pr.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(pr =>
                    EF.Functions.ILike(pr.Product.Name, $"%{search}%") ||
                    EF.Functions.ILike(pr.RequestNumber, $"%{search}%"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(pr => pr.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<PurchaseRequest?> GetByIdWithProductAsync(int id)
        {
            return await DbSet.Include(pr => pr.Product).FirstOrDefaultAsync(pr => pr.Id == id);
        }
    }
}
