namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    IUserRepository UserRepository { get; }
    ICartRepository CartRepository { get; }
    ICartProductRepository CartProductRepository { get; }
    ISaleRepository SaleRepository { get; }
    ISaleItemRepository SaleItemRepository { get; }
    IEventLogRepository EventLogRepository { get; }

    Task BeginTransactionAsync();
    Task SaveChangesAsync();
    Task RollbackAsync();
    Task CommitAsync();
}
