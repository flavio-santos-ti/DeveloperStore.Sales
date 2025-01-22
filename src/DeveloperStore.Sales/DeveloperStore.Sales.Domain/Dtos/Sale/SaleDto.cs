using DeveloperStore.Sales.Domain.Dtos.Sale;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Sale;

[ExcludeFromCodeCoverage]
public class SaleDto
{
    public int Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public int CustomerId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}
