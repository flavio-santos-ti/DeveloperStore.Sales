namespace DeveloperStore.Sales.Domain.Dtos.Cart;

public class RequestCartDto
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<RequestCartProductDto> Products { get; set; } = new List<RequestCartProductDto>();
}
