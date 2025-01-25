using DeveloperStore.Sales.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface IPostgreSqlDbContext
{
    DbSet<Product> Products { get; }
    DbSet<User> Users { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartProduct> CartProducts { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<T> Set<T>() where T : class; // Permite acesso aos DbSet genéricos
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
