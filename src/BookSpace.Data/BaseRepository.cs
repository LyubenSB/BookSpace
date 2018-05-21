using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BookSpace.Data.Contracts;
using Microsoft.EntityFrameworkCore;


namespace BookSpace.Data
{
    public class BaseRepository<TEntity> : IRepository<TEntity>
                                                    where TEntity : class
    {
        private readonly IDbContext dbContext;

        public BaseRepository(IDbContext dbCtx)
        {
            this.dbContext = dbCtx ?? throw new ArgumentNullException(nameof(dbCtx));
        }

        public async Task<TEntity> GetByIdAsync(object id)
        {
            return await this.dbContext.DbSet<TEntity>().FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await this.dbContext.DbSet<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> where)
        {
            return await this.dbContext.DbSet<TEntity>().Where(where).FirstOrDefaultAsync<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where)
        {
            return await this.dbContext.DbSet<TEntity>().Where(where).ToListAsync();
        }

        public async Task<IEnumerable<TProperty>> GetOneToManyAsync<TProperty>
                                   (Expression<Func<TEntity, bool>> where,
                                    Expression<Func<TEntity, IEnumerable<TProperty>>> selector)

        {

            return await this.dbContext.DbSet<TEntity>()
                   .Where(where)
                   .SelectMany(selector)
                   .ToListAsync();
        }

        public async Task<IEnumerable<TResultProperty>> GetManyToManyAsync<TProperty, TResultProperty>
                                    (Expression<Func<TEntity, bool>> where,
                                     Expression<Func<TEntity, IEnumerable<TProperty>>> selectorMany,
                                     Expression<Func<TProperty, TResultProperty>> selector)

        {

            return await this.dbContext.DbSet<TEntity>()
                   .Where(where)
                   .SelectMany(selectorMany)
                   .Select(selector)
                   .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> Search(Expression<Func<TEntity, bool>> filter)
        {
            return await this.dbContext.DbSet<TEntity>()
                    .Where(filter)
                    .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> SearchByNavigationProperty(string include, string includeProp,
                                     Expression<Func<TEntity, bool>> filter)
        {
            return await this.dbContext.DbSet<TEntity>()
                    .Include(include)
                    .Include(includeProp)
                    .Where(filter)
                    .ToListAsync();
        }

        public async Task<PagedResult<TEntity>> GetPaged(int page, int pageSize)
        {
            var result = new PagedResult<TEntity>();
            result.Page = page;
            result.PageSize = pageSize;

            var skip = (page - 1) * pageSize;
            result.Results = await this.dbContext.DbSet<TEntity>().Skip(skip).Take(pageSize).ToListAsync();

            return result;
        }

        public async Task<PagedResult<TResultProperty>> GetPagedManyToMany<TProperty, TResultProperty>
                                 (Expression<Func<TEntity, bool>> where,
                                  Expression<Func<TEntity, IEnumerable<TProperty>>> selectorMany,
                                  Expression<Func<TProperty, TResultProperty>> selector,
                                  int page, int pageSize) where TResultProperty : class

        {
            var result = new PagedResult<TResultProperty>();
            result.Page = page;
            result.PageSize = pageSize;

            var skip = (page - 1) * pageSize;

            result.Results = await this.dbContext.DbSet<TEntity>()
                   .Where(where)
                   .SelectMany(selectorMany)
                   .Select(selector)
                   .Skip(skip)
                   .Take(pageSize)
                   .ToListAsync();

            return result;
        }

        public async Task<int> GetCount()
        {
            return await this.dbContext.DbSet<TEntity>().CountAsync();
        }

        public async Task<IEnumerable<TEntity>> FindByExpressionOrdered<TProperty>
                            (Expression<Func<TEntity, TProperty>> order,
                            int recordsCount)
        {
            return await this.dbContext.DbSet<TEntity>()
                .OrderByDescending(order)
                .Take(recordsCount)
                .ToListAsync();
        }
    }
}
