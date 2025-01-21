using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Sale;

[ExcludeFromCodeCoverage]
public class RequestSaleDto
{
    public int CustomerId { get; set; }
    public string Branch { get; set; } = string.Empty;
    public List<RequestSaleItemDto> Items { get; set; } = new();
}
