using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
