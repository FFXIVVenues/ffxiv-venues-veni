using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Persistance.Abstraction;

namespace FFXIVVenues.Veni.Persistance
{

    public class InMemoryRepository : IRepository
    {

        private ConcurrentDictionary<Type, ConcurrentDictionary<string, IEntity>> _memoryStore;

        public InMemoryRepository()
        {
            _memoryStore = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IEntity>>();
        }

        public Task UpsertAsync<T>(T entity) where T : class, IEntity
        {
            var type = typeof(T);
            if (!_memoryStore.ContainsKey(type))
            {
                _memoryStore[type] = new ConcurrentDictionary<string, IEntity>();
            }
            _memoryStore[type][entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(T entity) where T : class, IEntity
        {
            this.DeleteAsync<T>(entity.Id);
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string id) where T : class, IEntity
        {
            var type = typeof(T);
            if (_memoryStore.ContainsKey(type))
                _memoryStore[type].TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public IQueryable<T> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity
        {
            var type = typeof(T);
            if (!_memoryStore.ContainsKey(type))
            {
                return Enumerable.Empty<T>().AsQueryable();
            }

            var func = predicate.Compile();
            return _memoryStore[type].Values
                .Select(item => item as T)
                .Where(item => item != null && func(item))
                .AsQueryable()!;
        }


        public IQueryable<T> GetAll<T>() where T : class, IEntity
        {
            var type = typeof(T);
            if (!_memoryStore.ContainsKey(type))
            {
                return Enumerable.Empty<T>().AsQueryable();
            }

            return _memoryStore[type].Values
                .Select(item => item as T)
                .Where(item => item != null)
                .AsQueryable()!;
        }

        public Task<T> GetByIdAsync<T>(string id) where T : class, IEntity
        {
            var type = typeof(T);
            if (!_memoryStore.ContainsKey(type))
            {
                return null;
            }

            return Task.FromResult(_memoryStore[type].TryGetValue(id, out var value) ? value as T : null);
        }

        public Task<bool> ExistsAsync<T>(string id) where T : class, IEntity
        {
            var type = typeof(T);
            if (!_memoryStore.ContainsKey(type))
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(_memoryStore[type].ContainsKey(id));
        }

    }

}
