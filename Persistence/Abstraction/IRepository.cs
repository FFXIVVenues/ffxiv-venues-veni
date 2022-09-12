using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Persistance.Abstraction
{
    public interface IRepository
    {
        Task UpsertAsync<T>(T entity) where T : class, IEntity;
        Task DeleteAsync<T>(T entity) where T : class, IEntity;
        Task DeleteAsync<T>(string id) where T : class, IEntity;
        Task<IQueryable<T>> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class, IEntity;
        Task<IQueryable<T>> GetAll<T>() where T : class, IEntity;
        Task<T> GetByIdAsync<T>(string id) where T : class, IEntity;
        Task<bool> ExistsAsync<T>(string id) where T : class, IEntity;
    }

}
