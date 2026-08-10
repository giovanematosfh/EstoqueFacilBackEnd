using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public GenericRepository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual IQueryable<TEntity> Query()
        {
            return DbSet.AsQueryable();
        }

        public virtual void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }
    }
}
