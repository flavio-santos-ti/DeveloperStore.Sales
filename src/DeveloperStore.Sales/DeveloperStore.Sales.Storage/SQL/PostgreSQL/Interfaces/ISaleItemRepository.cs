using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface ISaleItemRepository : IBaseRepository<SaleItem>
{
    Task<SaleItem?> GetByIdAsync(int id);
    Task<IEnumerable<SaleItem>> GetBySaleIdAsync(int saleId);
}
