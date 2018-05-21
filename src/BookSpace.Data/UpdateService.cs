using BookSpace.Data.Contracts;
using System;
using System.Threading.Tasks;

namespace BookSpace.Data
{
    public class UpdateService<TEntity> : IUpdateService<TEntity> 
                                                where TEntity : class
    {
        private readonly IDbContext dbContext;

        public UpdateService(IDbContext dbCtx)
        {
            this.dbContext = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
        }

        public async Task AddAsync(TEntity entity)
        {
            await this.dbContext.DbSet<TEntity>().AddAsync(entity);
            await this.dbContext.SaveAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            this.dbContext.DbSet<TEntity>().Remove(entity);
            await this.dbContext.SaveAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            this.dbContext.DbSet<TEntity>().Update(entity);
            await this.dbContext.SaveAsync();
        }
    }
}
