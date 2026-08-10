namespace EstoqueFacil.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        IQueryable<TEntity> Query();
        void Add(TEntity entity);
        void Remove(TEntity entity);
    }
}
