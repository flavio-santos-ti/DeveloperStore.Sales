using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using System.Net;
using System.Xml.Linq;

namespace DeveloperStore.Sales.Tests.Services;

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

        var expectedResponse = ApiResponseDto<UserDto>.AsSuccess(expectedUser);

        var userServiceMock = Substitute.For<IUserService>();
        userServiceMock.GetByIdAsync(userId).Returns(expectedResponse);

        // Act
        var result = await userServiceMock.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeTrue("a operação deve ser bem-sucedida quando o usuário é encontrado");
        result.StatusCode.Should().Be(200, "o status HTTP deve ser 200 quando o usuário é encontrado");
        result.Data.Should().NotBeNull("os dados do usuário devem ser retornados");
        result.Data.Should().BeEquivalentTo(expectedUser, "os dados retornados devem corresponder ao usuário esperado");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 99;
        var expectedMessage = $"Usuário com o ID {userId} não encontrado.";
        var expectedResponse = ApiResponseDto<UserDto>.AsNotFound(expectedMessage);

        var userServiceMock = Substitute.For<IUserService>();
        userServiceMock.GetByIdAsync(userId).Returns(expectedResponse);

        // Act
        var result = await userServiceMock.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeFalse("a operação deve falhar quando o usuário não é encontrado");
        result.StatusCode.Should().Be(404, "o status HTTP deve ser 404 quando o recurso não é encontrado");
        result.Message.Should().Be(expectedMessage, "a mensagem deve indicar que o usuário não foi encontrado");
        result.Data.Should().BeNull("os dados retornados devem ser nulos quando o usuário não é encontrado");
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

        var expectedResponse = ApiResponseDto<PagedResponseDto<UserDto>>.AsSuccess(pagedResponse);

        var userServiceMock = Substitute.For<IUserService>();
        userServiceMock.GetAllAsync(1, 10, null).Returns(expectedResponse);

        // Act
        var result = await userServiceMock.GetAllAsync(1, 10, null);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeTrue("a operação deve ser bem-sucedida quando usuários são encontrados");
        result.StatusCode.Should().Be(200, "o status HTTP deve ser 200 para operações bem-sucedidas");
        result.Data.Should().NotBeNull("os dados não devem ser nulos quando usuários são encontrados");
        result.Data!.Data.Should().HaveCount(mockUsers.Count, "a quantidade de usuários retornados deve ser igual à quantidade esperada");
        result.Data.CurrentPage.Should().Be(1, "a página atual deve ser a esperada");
        result.Data.TotalPages.Should().Be(1, "o número total de páginas deve ser igual ao esperado");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNotFound_WhenNoUsersExist()
    {
        // Arrange
        var userServiceMock = Substitute.For<IUserService>();
        var expectedResponse = ApiResponseDto<PagedResponseDto<UserDto>>.AsNotFound("Nenhum usuário encontrado.");

        userServiceMock.GetAllAsync(1, 10, null).Returns(expectedResponse);

        // Act
        var result = await userServiceMock.GetAllAsync(1, 10, null);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeFalse("a operação deve falhar quando nenhum usuário for encontrado");
        result.StatusCode.Should().Be(404, "o status HTTP deve ser 404 para recursos não encontrados");
        result.Message.Should().Be("Nenhum usuário encontrado.", "a mensagem deve indicar que nenhum usuário foi encontrado");
        result.Data.Should().BeNull("os dados devem ser nulos quando nenhum usuário for encontrado");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        int userId = 1;

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            Firstname = "Test",
            Lastname = "User",
            City = "Test City",
            Street = "Test Street",
            AddressNumber = 123,
            Zipcode = "12345-678",
            GeolocationLat = "0.0000",
            GeolocationLong = "0.0000",
            Phone = "123456789",
            Status = "Active",
            Role = "Customer"
        };

        var userDto = new UserDto
        {
            Email = user.Email,
            Username = user.Username,
            Name = new NameDto
            {
                Firstname = user.Firstname,
                Lastname = user.Lastname
            },
            Address = new AddressDto
            {
                City = user.City,
                Street = user.Street,
                Number = user.AddressNumber,
                Zipcode = user.Zipcode,
                Geolocation = new GeolocationDto
                {
                    Lat = user.GeolocationLat,
                    Long = user.GeolocationLong
                }
            },
            Phone = user.Phone,
            Status = user.Status,
            Role = user.Role
        };

        var userRepositoryMock = Substitute.For<IUserRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var mapperMock = Substitute.For<IMapper>();
        var validatorMock = Substitute.For<IValidator<RequestUserDto>>();

        userRepositoryMock.GetByIdAsync(userId).Returns(user);
        mapperMock.Map<UserDto>(user).Returns(userDto);

        var userService = new UserService(userRepositoryMock, mapperMock, validatorMock, unitOfWorkMock);

        // Act
        var result = await userService.DeleteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be($"Usuário com o ID {userId} excluído com sucesso.");
        result.Data.Should().BeEquivalentTo(userDto);

        // Verify
        await userRepositoryMock.Received(1).GetByIdAsync(userId);
        await userRepositoryMock.Received(1).DeleteAsync(user);
        await unitOfWorkMock.Received(1).BeginTransactionAsync();
        await unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 999;
        var expectedMessage = $"Usuário com o ID {userId} não encontrado.";

        var userRepositoryMock = Substitute.For<IUserRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var mapperMock = Substitute.For<IMapper>();
        var validatorMock = Substitute.For<IValidator<RequestUserDto>>();

        userRepositoryMock.GetByIdAsync(userId).Returns((User?)null);

        var userService = new UserService(userRepositoryMock, mapperMock, validatorMock, unitOfWorkMock);

        // Act
        var result = await userService.DeleteAsync(userId);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeFalse("o resultado deve indicar falha, pois o usuário não existe");
        result.StatusCode.Should().Be(404, "o código de status deve indicar recurso não encontrado");
        result.Message.Should().Be(expectedMessage, "a mensagem deve informar que o usuário não foi encontrado");
        result.Data.Should().BeNull("os dados devem ser nulos quando o usuário não existe");

        // Verify
        await userRepositoryMock.Received(1).GetByIdAsync(userId); 
        await userRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<User>()); 
        await unitOfWorkMock.DidNotReceive().BeginTransactionAsync(); 
        await unitOfWorkMock.DidNotReceive().CommitAsync(); 
    }
}
