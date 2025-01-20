namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task SaveChangesAsync();
    Task RollbackAsync();
    Task CommitAsync();
}
