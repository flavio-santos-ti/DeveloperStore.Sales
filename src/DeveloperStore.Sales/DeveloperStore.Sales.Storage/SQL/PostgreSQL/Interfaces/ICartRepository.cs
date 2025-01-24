using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface ICartRepository : IBaseRepository<Cart>
{
    Task<Cart?> GetByIdAsync(int id);
    IQueryable<Cart> GetAllQueryable();
}
