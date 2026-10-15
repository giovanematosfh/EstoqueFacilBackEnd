using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IStockMovementRepository StockMovements { get; }
        ISupplierRepository Suppliers { get; }
        IBranchRepository Branches { get; }
        IProductStockRepository ProductStocks { get; }
        IPurchaseRequestRepository PurchaseRequests { get; }
        IGenericRepository<Plan> Plans { get; }
        IGenericRepository<Company> Companies { get; }

        Task<int> CommitAsync();
    }
}
