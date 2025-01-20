using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task UpdateAsync(User user);

}
