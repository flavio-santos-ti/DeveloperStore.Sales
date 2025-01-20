using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Domain.Dtos.Product;

public class RequestProductDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRating Rating { get; set; } = new ProductRating();
}
