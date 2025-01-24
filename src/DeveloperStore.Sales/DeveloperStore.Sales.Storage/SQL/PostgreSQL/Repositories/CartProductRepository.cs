using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class CartProductRepository : BaseRepository<CartProduct>, ICartProductRepository
{
    public CartProductRepository(PostgreSqlDbContext context) : base(context) { }

    public async Task<CartProduct?> GetByIdAsync(int id)
    {
        return await _context.CartProducts
            .Include(cp => cp.Product)
            .FirstOrDefaultAsync(cp => cp.Id == id);
    }

    public async Task<IEnumerable<CartProduct>> GetByCartIdAsync(int cartId)
    {
        return await _context.CartProducts
            .Where(cp => cp.CartId == cartId)
            .Include(cp => cp.Product)
            .ToListAsync();
    }
}
