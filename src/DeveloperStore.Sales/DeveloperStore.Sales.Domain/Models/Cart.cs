using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Models;

[ExcludeFromCodeCoverage]
public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
}
