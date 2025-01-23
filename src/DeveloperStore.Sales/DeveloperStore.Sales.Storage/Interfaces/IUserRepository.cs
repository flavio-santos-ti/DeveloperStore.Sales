using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    IQueryable<User> GetAllQueryable();
    Task<User?> GetByUsernameAsync(string username);
}
