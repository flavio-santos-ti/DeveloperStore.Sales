using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    public SaleRepository(IPostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<Sale?> GetByIdAsync(int id)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
