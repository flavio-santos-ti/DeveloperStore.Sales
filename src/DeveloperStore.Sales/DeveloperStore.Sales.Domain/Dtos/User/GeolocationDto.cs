using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.User;

[ExcludeFromCodeCoverage]
public class GeolocationDto
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}
