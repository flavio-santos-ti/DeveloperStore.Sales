using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class SaleItemRepository : BaseRepository<SaleItem>, ISaleItemRepository
{
    public SaleItemRepository(ApplicationDbContext context) : base(context)
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
