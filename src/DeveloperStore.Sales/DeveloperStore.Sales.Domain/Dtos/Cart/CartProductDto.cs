using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Cart;

[ExcludeFromCodeCoverage]
public class CartProductDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
