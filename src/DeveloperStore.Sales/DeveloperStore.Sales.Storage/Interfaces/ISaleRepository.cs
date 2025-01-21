using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ISaleRepository
{
    Task AddAsync(Sale sale);
    Task<Sale?> GetByIdAsync(int id);
    Task<IEnumerable<Sale>> GetAllAsync();
    Task UpdateAsync(Sale sale);
    Task DeleteAsync(Sale sale);
}
