using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Domain.Dtos.Auth;

[ExcludeFromCodeCoverage]
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
}
