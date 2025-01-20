using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task DeleteAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> ExistsByTitleAsync(string title);
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    IQueryable<Product> GetAllQueryable();
}
