using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;

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
}
