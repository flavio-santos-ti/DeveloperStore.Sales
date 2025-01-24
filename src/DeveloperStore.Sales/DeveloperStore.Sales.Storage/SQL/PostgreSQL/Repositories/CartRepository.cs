using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(IPostgreSqlDbContext context) : base(context) { }

    public async Task<Cart?> GetByIdAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.CartProducts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public IQueryable<Cart> GetAllQueryable()
    {
        return _context.Carts.Include(c => c.CartProducts); // Inclui os produtos do carrinho
    }
}
