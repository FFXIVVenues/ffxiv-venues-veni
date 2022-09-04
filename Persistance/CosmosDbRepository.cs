using FFXIVVenues.Veni.Persistance.Abstraction;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Persistance
{

    public class CosmosDbRepository : IRepository, IDisposable
    {
        private Database _database;

        public CosmosDbRepository(string strConnectionString)
        {
            var client = new CosmosClient(strConnectionString);
            this._database = client.GetDatabase("ffxivvenues-veni-ki");
        }

        public Task UpsertAsync<T>(T entity) where T : class, IEntity =>
            _database.GetContainer(typeof(T).Name).UpsertItemAsync(entity, PartitionKey.None);

        public Task DeleteAsync<T>(T entity) where T : class, IEntity =>
            this.DeleteAsync<T>(entity.Id.ToString());

        public Task DeleteAsync<T>(string id) where T : class, IEntity =>
            _database.GetContainer(typeof(T).Name).DeleteItemAsync<T>(id, PartitionKey.None);

        public IQueryable<T> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity =>
            _database.GetContainer(typeof(T).Name).GetItemLinqQueryable<T>().Where(predicate);

        public IQueryable<T> GetAll<T>() where T : class, IEntity =>
            _database.GetContainer(typeof(T).Name).GetItemLinqQueryable<T>();

        public Task<T> GetByIdAsync<T>(string id) where T : class, IEntity => 
            _database.GetContainer(typeof(T).Name).ReadItemAsync<T>(id, PartitionKey.None).ContinueWith(t => t.Result.Resource);

        public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity =>
            this.GetByIdAsync<T>(id).ContinueWith(e => e != null);

        public void Dispose() =>
            GC.SuppressFinalize(this);

        ~CosmosDbRepository() =>
            Dispose();

    }
}
