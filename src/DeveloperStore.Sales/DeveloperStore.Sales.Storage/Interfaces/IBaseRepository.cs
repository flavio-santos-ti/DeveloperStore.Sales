namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
    Task UpdateAsync(T entity);
}
