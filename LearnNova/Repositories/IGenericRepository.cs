using System.Linq;
using System.Linq.Expressions;

namespace LearnNova.Repositories;

/// <summary>Base CRUD contract for any entity, keyed by <typeparamref name="TKey"/>.</summary>
public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> GetQueryable();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}

/// <summary>Convenience alias for the common case: an entity keyed by <see langword="int"/>.</summary>
public interface IGenericRepository<T> : IGenericRepository<T, int> where T : class
{
}


