using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MT.AzureStorageLib.Services.Interfaces
{
    public interface INoSqlStorage <TEntity>
    {
        Task<TEntity> AddAsync(TEntity entity);

        Task DeleteAsync(string rowKey, string partitionKey);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task<TEntity> GetAsync(string rowKey, string partitionKey);

        IQueryable<TEntity> All();

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query);
    }
}
