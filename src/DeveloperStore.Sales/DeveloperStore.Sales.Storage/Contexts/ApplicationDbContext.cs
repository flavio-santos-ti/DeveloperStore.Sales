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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductMap());

    }
}
