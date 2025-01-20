namespace DeveloperStore.Sales.Domain.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
}
