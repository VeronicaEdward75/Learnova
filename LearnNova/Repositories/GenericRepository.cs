using System.Linq.Expressions;
using LearnNova.Data;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Repositories;

public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class
{
    // protected (not private) so subclasses like UserRepository can run their own
    // queries against the same DbSet instead of re-injecting ApplicationDbContext.
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _set;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    // virtual: subclasses override these when they need eager-loading (.Include) or
    // default ordering that doesn't belong in every entity's generic base behaviour.
    public virtual async Task<T?> GetByIdAsync(TKey id) => await _set.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}

/// <summary>Convenience alias for the common case: an entity keyed by <see langword="int"/>.</summary>
public class GenericRepository<T> : GenericRepository<T, int>, IGenericRepository<T> where T : class
{
    public GenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
