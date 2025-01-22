using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task<Cart?> GetByIdAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.CartProducts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task DeleteAsync(Cart cart)
    {
        _context.Carts.Remove(cart);
        await Task.CompletedTask;
    }

    public IQueryable<Cart> GetAllQueryable()
    {
        return _context.Carts.Include(c => c.CartProducts); // Inclui os produtos do carrinho
    }

    public async Task UpdateAsync(Cart cart)
    {
        _context.Carts.Update(cart);
        await Task.CompletedTask;
    }
}
