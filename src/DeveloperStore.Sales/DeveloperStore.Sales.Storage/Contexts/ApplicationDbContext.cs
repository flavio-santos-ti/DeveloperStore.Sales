using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Mappings;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartProduct> CartProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductMap());
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new CartMap());
        modelBuilder.ApplyConfiguration(new CartProductMap());
    }
}
