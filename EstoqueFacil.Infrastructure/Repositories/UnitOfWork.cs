using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Infrastructure.Data;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private ICategoryRepository? _categories;
        private IProductRepository? _products;
        private IStockMovementRepository? _stockMovements;
        private ISupplierRepository? _suppliers;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        public IProductRepository Products => _products ??= new ProductRepository(_context);

        public IStockMovementRepository StockMovements => _stockMovements ??= new StockMovementRepository(_context);

        public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);

        public Task<int> CommitAsync()
        {
            return _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
