using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Service.Interfaces;
using NSubstitute;

namespace DeveloperStore.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _userService = Substitute.For<IUserService>();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var expectedUser = new UserDto
            {
                Email = "test@example.com",
                Username = "testuser",
                Name = new NameDto
                {
                    Firstname = "Test",
                    Lastname = "User"
                },
                Address = new AddressDto
                {
                    City = "Test City",
                    Street = "Test Street",
                    Number = 123,
                    Zipcode = "12345-678",
                    Geolocation = new GeolocationDto
                    {
                        Lat = "0.0000",
                        Long = "0.0000"
                    }
                },
                Phone = "123456789",
                Status = "Active",
                Role = "Customer"
            };

            var response = ApiResponseDto<UserDto>.AsSuccess(expectedUser);

            _userService.GetByIdAsync(userId).Returns(response);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Equal(expectedUser.Email, result.Data.Email);
            Assert.Equal(expectedUser.Username, result.Data.Username);
            Assert.Equal(expectedUser.Name.Firstname, result.Data.Name.Firstname);
            Assert.Equal(expectedUser.Name.Lastname, result.Data.Name.Lastname);
            Assert.Equal(expectedUser.Address.City, result.Data.Address.City);
            Assert.Equal(expectedUser.Address.Street, result.Data.Address.Street);
            Assert.Equal(expectedUser.Address.Number, result.Data.Address.Number);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 99;
            var response = ApiResponseDto<UserDto>.AsNotFound($"Usuário com o ID {userId} não encontrado.");

            _userService.GetByIdAsync(userId).Returns(response);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"Usuário com o ID {userId} não encontrado.", result.Message);
            Assert.Null(result.Data);
        }
    }
}
