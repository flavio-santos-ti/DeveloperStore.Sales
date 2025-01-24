using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

namespace DeveloperStore.Sales.Storage.UnitOfWork;

public interface IUnitOfWork
{
    IProductRepository ProductRepository { get; }
    IUserRepository UserRepository { get; }
    ICartRepository CartRepository { get; }
    ICartProductRepository CartProductRepository { get; }
    ISaleRepository SaleRepository { get; }
    ISaleItemRepository SaleItemRepository { get; }
    IEventLogMongoDbRepository EventLogRepository { get; }

    Task BeginTransactionAsync();
    Task SaveChangesAsync();
    Task RollbackAsync();
    Task CommitAsync();
}
