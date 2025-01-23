using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ISaleItemRepository : IBaseRepository<SaleItem>
{
    Task<SaleItem?> GetByIdAsync(int id);
    Task<IEnumerable<SaleItem>> GetBySaleIdAsync(int saleId);
}
