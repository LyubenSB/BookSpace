using System.Threading.Tasks;

namespace BookSpace.Data.Contracts
{
    public interface IUpdateService<TEntity>
                    where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}
