using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Sale;

[ExcludeFromCodeCoverage]
public class SaleItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
}
