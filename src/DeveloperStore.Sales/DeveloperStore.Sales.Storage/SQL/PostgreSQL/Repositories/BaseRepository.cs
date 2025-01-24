using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly IPostgreSqlDbContext _context;

    protected BaseRepository(IPostgreSqlDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _context.Set<T>().AddAsync(entity); 
    }

    public async Task DeleteAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _context.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _context.Set<T>().Update(entity);
        await Task.CompletedTask;
    }
}
