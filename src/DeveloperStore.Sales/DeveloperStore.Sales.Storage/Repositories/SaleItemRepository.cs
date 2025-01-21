using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Storage.Contexts;
using DeveloperStore.Sales.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Storage.Repositories;

public class SaleItemRepository : ISaleItemRepository
{
    private readonly ApplicationDbContext _context;

    public SaleItemRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(SaleItem saleItem)
    {
        await _context.SaleItems.AddAsync(saleItem);
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

    public async Task UpdateAsync(SaleItem saleItem)
    {
        _context.SaleItems.Update(saleItem);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(SaleItem saleItem)
    {
        _context.SaleItems.Remove(saleItem);
        await Task.CompletedTask;
    }
}
