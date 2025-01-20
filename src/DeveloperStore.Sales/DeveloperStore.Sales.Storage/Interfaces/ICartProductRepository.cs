using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ICartProductRepository
{
    Task AddAsync(CartProduct cartProduct);
    Task<CartProduct?> GetByIdAsync(int id);
    Task<IEnumerable<CartProduct>> GetByCartIdAsync(int cartId);
    Task DeleteAsync(CartProduct cartProduct);
    Task UpdateAsync(CartProduct cartProduct);
}
