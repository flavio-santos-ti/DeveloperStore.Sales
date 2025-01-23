using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Models;

[ExcludeFromCodeCoverage]
public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public int CustomerId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}
