using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Models;

[ExcludeFromCodeCoverage]
public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
}
