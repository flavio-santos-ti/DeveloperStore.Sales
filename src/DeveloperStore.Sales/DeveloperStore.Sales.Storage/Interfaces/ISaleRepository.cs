using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ISaleRepository : IBaseRepository<Sale>
{
    Task<Sale?> GetByIdAsync(int id);
}
