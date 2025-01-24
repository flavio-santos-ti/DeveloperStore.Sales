using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface ICartProductRepository : IBaseRepository<CartProduct>
{
    Task<CartProduct?> GetByIdAsync(int id);
    Task<IEnumerable<CartProduct>> GetByCartIdAsync(int cartId);
}
