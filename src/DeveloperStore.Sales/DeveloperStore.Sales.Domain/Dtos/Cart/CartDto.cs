using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Cart;

[ExcludeFromCodeCoverage]
public class CartDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public ICollection<CartProductDto> Products { get; set; } = new List<CartProductDto>();
}
