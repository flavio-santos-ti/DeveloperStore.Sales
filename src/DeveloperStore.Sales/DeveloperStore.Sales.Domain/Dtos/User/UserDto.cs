namespace DeveloperStore.Sales.Domain.Dtos.User;

public class UserDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public NameDto Name { get; set; } = new NameDto();
    public AddressDto Address { get; set; } = new AddressDto();
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
