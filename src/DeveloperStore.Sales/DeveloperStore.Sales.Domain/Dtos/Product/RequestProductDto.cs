using DeveloperStore.Sales.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Product;

[ExcludeFromCodeCoverage]
public class RequestProductDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRating Rating { get; set; } = new ProductRating();
}
