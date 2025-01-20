using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class CartProductRepository : ICartProductRepository
{
    private readonly ApplicationDbContext _context;

    public CartProductRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(CartProduct cartProduct)
    {
        await _context.CartProducts.AddAsync(cartProduct);
    }

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

    public async Task DeleteAsync(CartProduct cartProduct)
    {
        _context.CartProducts.Remove(cartProduct);
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(CartProduct cartProduct)
    {
        _context.CartProducts.Update(cartProduct);
        await Task.CompletedTask;
    }
}
