using Microsoft.Azure.Cosmos.Table;
using MT.AzureStorageLib.Services.Interfaces;

using System.Linq.Expressions;


namespace MT.AzureStorageLib.Services.Concrete
{
    public class TableStorage<TEntity> : INoSqlStorage<TEntity> where TEntity : TableEntity, new()
    {
        private readonly CloudTableClient _cloudTableClient;

        private readonly CloudTable _table;

        public TableStorage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString.AzureStorageConnectionString);

            _cloudTableClient = storageAccount.CreateCloudTableClient();

            _table = _cloudTableClient.GetTableReference(typeof(TEntity).Name);

            _table.CreateIfNotExists();
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);

            var execute = await _table.ExecuteAsync(operation);

            return execute.Result as TEntity;
        }

        public IQueryable<TEntity> All()
        {
            return _table.CreateQuery<TEntity>().AsQueryable();
        }

        public async Task DeleteAsync(string rowKey, string partitionKey)
        {
            var entity = await GetAsync(rowKey, partitionKey);

            var operation = TableOperation.Delete(entity);

            await _table.ExecuteAsync(operation);
        }

        public async Task<TEntity> GetAsync(string rowKey, string partitionKey)
        {
            var operation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

            var execute = await _table.ExecuteAsync(operation);

            return execute.Result as TEntity;
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return _table.CreateQuery<TEntity>().Where(query);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var operation = TableOperation.Replace(entity);

            var execute = await _table.ExecuteAsync(operation);

            return execute.Result as TEntity;
        }
    }


}
