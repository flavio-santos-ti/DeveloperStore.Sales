using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    IQueryable<User> GetAllQueryable();
}
