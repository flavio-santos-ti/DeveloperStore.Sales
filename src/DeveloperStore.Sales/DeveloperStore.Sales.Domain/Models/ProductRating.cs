using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Models;

[ExcludeFromCodeCoverage]
public class ProductRating
{
    public decimal? Rate { get; set; }
    public int? Count { get; set; }
}
