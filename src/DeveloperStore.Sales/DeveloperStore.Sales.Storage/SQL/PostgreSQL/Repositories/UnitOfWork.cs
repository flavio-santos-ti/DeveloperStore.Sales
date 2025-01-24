using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Repositories;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostgreSqlDbContext _context;
    private IDbContextTransaction? _transaction;
    private IProductRepository? _productRepository;
    private IUserRepository? _userRepository;
    private ICartRepository? _cartRepository;
    private ICartProductRepository? _cartProductRepository;
    private ISaleRepository? _saleRepository;
    private ISaleItemRepository? _saleItemRepository;
    private readonly IMongoDbContext _mongoDbContext;
    private IEventLogMongoDbRepository? _eventLogRepository;

    public UnitOfWork(PostgreSqlDbContext context, IMongoDbContext mongoDbContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mongoDbContext = mongoDbContext ?? throw new ArgumentNullException(nameof(mongoDbContext));
    }

    public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);
    public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
    public ICartRepository CartRepository => _cartRepository ??= new CartRepository(_context);
    public ICartProductRepository CartProductRepository => _cartProductRepository ??= new CartProductRepository(_context);
    public ISaleRepository SaleRepository => _saleRepository ??= new SaleRepository(_context);
    public ISaleItemRepository SaleItemRepository => _saleItemRepository ??= new SaleItemRepository(_context);
    public IEventLogMongoDbRepository EventLogRepository => _eventLogRepository ??= new EventLogMongoDbRepository(_mongoDbContext);

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Uma transação já está ativa.");
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }
}
