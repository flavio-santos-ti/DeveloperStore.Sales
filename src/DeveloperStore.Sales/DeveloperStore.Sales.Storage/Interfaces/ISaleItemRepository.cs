using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ISaleItemRepository
{
    Task AddAsync(SaleItem saleItem);
    Task<SaleItem?> GetByIdAsync(int id);
    Task<IEnumerable<SaleItem>> GetBySaleIdAsync(int saleId);
    Task UpdateAsync(SaleItem saleItem);
    Task DeleteAsync(SaleItem saleItem);
}
