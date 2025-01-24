using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlDbContext context) : base(context) { }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        return await _context.Products.AnyAsync(p => p.Title.ToLower() == title.ToLower());
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Rating)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public IQueryable<Product> GetAllQueryable()
    {
        return _context.Products.AsQueryable();
    }
}
