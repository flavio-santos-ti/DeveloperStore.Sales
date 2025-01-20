using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface ICartRepository
{
    Task AddAsync(Cart cart);
}
