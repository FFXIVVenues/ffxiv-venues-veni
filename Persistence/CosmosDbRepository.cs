using FFXIVVenues.Veni.Persistance.Abstraction;
using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Persistance
{

    public class CosmosDbRepository : IRepository, IDisposable
    {
        private Database _database;
        private Dictionary<string, Container> _containerCache = new();

        public CosmosDbRepository(string strConnectionString)
        {
            var client = new CosmosClient(strConnectionString);
            this._database = client.GetDatabase("ffxivvenues-veni-ki");
        }

        public async Task UpsertAsync<T>(T entity) where T : class, IEntity
        {
            var container = await GetContainer(typeof(T).Name);
            await container.UpsertItemAsync(entity, PartitionKey.None);
        }

        public Task DeleteAsync<T>(T entity) where T : class, IEntity =>
            this.DeleteAsync<T>(entity.id.ToString());

        public async Task DeleteAsync<T>(string id) where T : class, IEntity
        {
            var container = await GetContainer(typeof(T).Name);
            await container.DeleteItemAsync<T>(id, PartitionKey.None);
        }

        public async Task<IQueryable<T>> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
        {
            var container = await GetContainer(typeof(T).Name);
            return container.GetItemLinqQueryable<T>(true).Where(predicate);
        }

        public async Task<IQueryable<T>> GetAll<T>() where T : class, IEntity
        {
            var container = await GetContainer(typeof(T).Name);
            return container.GetItemLinqQueryable<T>(true).AsQueryable();
        }

        public async Task<T> GetByIdAsync<T>(string id) where T : class, IEntity
        {
            try {
                var container = await GetContainer(typeof(T).Name);
                return await container.ReadItemAsync<T>(id, PartitionKey.None);
            } catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
                return null;
            }
        }

        public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity =>
            this.GetByIdAsync<T>(id).ContinueWith(e => e != null);

        private async Task<Container> GetContainer(string name)
        {
            if (this._containerCache.TryGetValue(name, out var container))
                return container;
            var result = await this._database.CreateContainerIfNotExistsAsync(new() { Id = name, PartitionKeyPath = "/Id" });
            return _containerCache[name] = result.Container;
        }

        public void Dispose() =>
            GC.SuppressFinalize(this);

        ~CosmosDbRepository() =>
            Dispose();

    }
}
