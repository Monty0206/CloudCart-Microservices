using MongoDB.Driver;
using System.Linq.Expressions;

namespace Common.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> CreateAsync(T entity);
    Task<bool> UpdateAsync(string id, T entity);
    Task<bool> DeleteAsync(string id);
    Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<(IEnumerable<T> items, long totalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null);
}
