using CorpNetMessenger.Domain.Entities;
using System.Linq.Expressions;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(string id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}
