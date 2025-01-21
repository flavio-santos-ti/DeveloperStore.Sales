using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Cart;

[ExcludeFromCodeCoverage]
public class RequestCartDto
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<RequestCartProductDto> Products { get; set; } = new List<RequestCartProductDto>();
}
