using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;

public class SaleItemRepository : BaseRepository<SaleItem>, ISaleItemRepository
{
    public SaleItemRepository(PostgreSqlDbContext context) : base(context)
    {
    }

    public async Task<SaleItem?> GetByIdAsync(int id)
    {
        return await _context.SaleItems
            .FirstOrDefaultAsync(si => si.Id == id);
    }

    public async Task<IEnumerable<SaleItem>> GetBySaleIdAsync(int saleId)
    {
        return await _context.SaleItems
            .Where(si => si.SaleId == saleId)
            .ToListAsync();
    }
}
