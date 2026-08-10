namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IStockMovementRepository StockMovements { get; }
        ISupplierRepository Suppliers { get; }

        Task<int> CommitAsync();
    }
}
