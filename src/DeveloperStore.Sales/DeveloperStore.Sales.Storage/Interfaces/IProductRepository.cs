﻿using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Storage.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> ExistsByTitleAsync(string title);
    Task<Product?> GetByIdAsync(int id);
    IQueryable<Product> GetAllQueryable();
}
