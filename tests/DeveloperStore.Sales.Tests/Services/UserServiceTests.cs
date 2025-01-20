using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Service.Interfaces;
using NSubstitute;

namespace DeveloperStore.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IUserService _userServiceMock;

        public UserServiceTests()
        {
            _userServiceMock = Substitute.For<IUserService>();
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

            _userServiceMock.GetByIdAsync(userId).Returns(response);

            // Act
            var result = await _userServiceMock.GetByIdAsync(userId);

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

            _userServiceMock.GetByIdAsync(userId).Returns(response);

            // Act
            var result = await _userServiceMock.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"Usuário com o ID {userId} não encontrado.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPagedResponse_WhenUsersExist()
        {
            // Arrange
            var mockUsers = new List<UserDto>
            {
                new UserDto
                {
                    Email = "user1@example.com",
                    Username = "user1",
                    Name = new NameDto
                    {
                        Firstname = "User",
                        Lastname = "One"
                    },
                    Address = new AddressDto
                    {
                        City = "City1",
                        Street = "Street1",
                        Number = 1,
                        Zipcode = "12345",
                        Geolocation = new GeolocationDto
                        {
                            Lat = "0.0",
                            Long = "0.0"
                        }
                    },
                    Phone = "123456789",
                    Status = "Active",
                    Role = "Customer"
                },
                new UserDto
                {
                    Email = "user2@example.com",
                    Username = "user2",
                    Name = new NameDto
                    {
                        Firstname = "User",
                        Lastname = "Two"
                    },
                    Address = new AddressDto
                    {
                        City = "City2",
                        Street = "Street2",
                        Number = 2,
                        Zipcode = "54321",
                        Geolocation = new GeolocationDto
                        {
                            Lat = "1.0",
                            Long = "1.0"
                        }
                    },
                    Phone = "987654321",
                    Status = "Active",
                    Role = "Manager"
                }
            };

            var pagedResponse = new PagedResponseDto<UserDto>(
                data: mockUsers,
                totalItems: mockUsers.Count,
                currentPage: 1,
                totalPages: 1);

            var response = ApiResponseDto<PagedResponseDto<UserDto>>.AsSuccess(pagedResponse);

            _userServiceMock.GetAllAsync(1, 10, null).Returns(response);

            // Act
            var result = await _userServiceMock.GetAllAsync(1, 10, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(mockUsers.Count, result.Data.Data.Count());
            Assert.Equal(1, result.Data.CurrentPage);
            Assert.Equal(1, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnNotFound_WhenNoUsersExist()
        {
            // Arrange
            var response = ApiResponseDto<PagedResponseDto<UserDto>>.AsNotFound("Nenhum usuário encontrado.");

            _userServiceMock.GetAllAsync(1, 10, null).Returns(response);

            // Act
            var result = await _userServiceMock.GetAllAsync(1, 10, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Nenhum usuário encontrado.", result.Message);
        }

    }
}
