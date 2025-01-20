using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeveloperStore.Sales.Storage.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IProductRepository? _productRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);

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
