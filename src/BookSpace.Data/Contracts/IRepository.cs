using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BookSpace.Data.Contracts
{
    public interface IRepository<TEntity> 
                     where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> where);

        Task<TEntity> GetByExpressionAsync(Expression<Func<TEntity, bool>> where);

        Task<IEnumerable<TProperty>> GetOneToManyAsync<TProperty>
                                   (Expression<Func<TEntity, bool>> where,
                                    Expression<Func<TEntity, IEnumerable<TProperty>>> selector);

        Task<IEnumerable<TResultProperty>> GetManyToManyAsync<TProperty, TResultProperty>
                                    (Expression<Func<TEntity, bool>> where,
                                     Expression<Func<TEntity, IEnumerable<TProperty>>> selectorMany,
                                     Expression<Func<TProperty, TResultProperty>> selector);

        Task<IEnumerable<TEntity>> Search(Expression<Func<TEntity, bool>> filter);

        Task<IEnumerable<TEntity>> SearchByNavigationProperty(string include, string includeProp,
                                     Expression<Func<TEntity, bool>> filter);

        Task<PagedResult<TEntity>> GetPaged(int page, int pageSize);

        Task<PagedResult<TResultProperty>> GetPagedManyToMany<TProperty, TResultProperty>
                                  (Expression<Func<TEntity, bool>> where,
                                   Expression<Func<TEntity, IEnumerable<TProperty>>> selectorMany,
                                   Expression<Func<TProperty, TResultProperty>> selector,
                                   int page, int pageSize) where TResultProperty : class;
 
         Task<IEnumerable<TEntity>> FindByExpressionOrdered<TProperty>
                                    (Expression<Func<TEntity, TProperty>> order,
                                     int recordsCount);

        Task<int> GetCount();
    }
}
