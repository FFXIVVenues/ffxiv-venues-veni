using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using Microsoft.Azure.Cosmos;

namespace FFXIVVenues.Veni.Infrastructure.Persistence
{

    public class CosmosDbRepository : IRepository, IDisposable
    {
        private readonly Database _database;
        private readonly Dictionary<string, Container> _containerCache = new();
        private readonly RollingCacheSet _cache = new ();

        public CosmosDbRepository(string strConnectionString)
        {
            var client = new CosmosClient(strConnectionString);
            this._database = client.GetDatabase("ffxivvenues-veni-ki");
        }

        public async Task UpsertAsync<T>(T entity) where T : class, IEntity
        {
            this._cache.For<T>().Remove(entity.id);
            var container = await GetContainer(typeof(T).Name);
            await container.UpsertItemAsync(entity, PartitionKey.None);
        }

        public Task DeleteAsync<T>(T entity) where T : class, IEntity =>
            this.DeleteAsync<T>(entity.id.ToString());

        public async Task DeleteAsync<T>(string id) where T : class, IEntity
        {
            this._cache.For<T>().Remove(id);
            var container = await GetContainer(typeof(T).Name);
            await container.DeleteItemAsync<T>(id, PartitionKey.None);
        }

        public async Task<IQueryable<T>> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
        {
            predicate.ToString();
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
            try
            {
                var cacheResult = this._cache.For<T>().Get(id);
                if (cacheResult.Result == CacheResult.CacheHit)
                    return cacheResult.Value;
                var container = await GetContainer(typeof(T).Name);
                var result = await container.ReadItemAsync<T>(id, PartitionKey.None);
                this._cache.For<T>().Set(id, result);
                return result;
            } catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
                this._cache.For<T>().Set(id, null);
                return null;
            }
        }

        public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity =>
            this.GetByIdAsync<T>(id).ContinueWith(e => e.IsCompletedSuccessfully && e.Result != null);

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
