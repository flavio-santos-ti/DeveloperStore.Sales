namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
    Task UpdateAsync(T entity);
}
