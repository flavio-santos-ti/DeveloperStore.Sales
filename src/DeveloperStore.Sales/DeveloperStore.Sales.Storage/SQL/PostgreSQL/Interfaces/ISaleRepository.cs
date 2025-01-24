using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface ISaleRepository : IBaseRepository<Sale>
{
    Task<Sale?> GetByIdAsync(int id);
}
