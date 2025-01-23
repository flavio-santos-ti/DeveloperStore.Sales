using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context) { }


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
