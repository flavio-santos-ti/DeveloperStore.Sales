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

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenUserIsUpdated()
    {
        // Arrange
        int userId = 1;
        var requestUserDto = new RequestUserDto
        {
            Email = "updated@example.com",
            Username = "updateduser",
            Password = "UpdatedPassword123",
            Name = new NameDto
            {
                Firstname = "Updated",
                Lastname = "User"
            },
            Address = new AddressDto
            {
                City = "Updated City",
                Street = "Updated Street",
                Number = 999,
                Zipcode = "99999-999",
                Geolocation = new GeolocationDto
                {
                    Lat = "99.9999",
                    Long = "99.9999"
                }
            },
            Phone = "999999999",
            Status = "Active",
            Role = "Manager"
        };

        var existingUser = new User
        {
            Id = userId,
            Email = "original@example.com",
            Username = "originaluser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OriginalPassword"),
            Firstname = "Original",
            Lastname = "User",
            City = "Original City",
            Street = "Original Street",
            AddressNumber = 123,
            Zipcode = "12345-678",
            GeolocationLat = "0.0000",
            GeolocationLong = "0.0000",
            Phone = "123456789",
            Status = "Active",
            Role = "Customer"
        };

        var updatedUserDto = new UserDto
        {
            Email = requestUserDto.Email,
            Username = requestUserDto.Username,
            Name = requestUserDto.Name,
            Address = requestUserDto.Address,
            Phone = requestUserDto.Phone,
            Status = requestUserDto.Status,
            Role = requestUserDto.Role
        };

        var userRepositoryMock = Substitute.For<IUserRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var mapperMock = Substitute.For<IMapper>();
        var validatorMock = Substitute.For<IValidator<RequestUserDto>>();

        userRepositoryMock.GetByIdAsync(userId).Returns(existingUser);
        validatorMock.ValidateAsync(requestUserDto).Returns(new FluentValidation.Results.ValidationResult());
        mapperMock.Map(requestUserDto, existingUser).Returns(existingUser);
        mapperMock.Map<UserDto>(existingUser).Returns(updatedUserDto);

        var userService = new UserService(userRepositoryMock, mapperMock, validatorMock, unitOfWorkMock);

        // Act
        var result = await userService.UpdateAsync(userId, requestUserDto);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeTrue("a operação deve ser bem-sucedida");
        result.StatusCode.Should().Be(200, "o status HTTP deve ser 200 quando a atualização é bem-sucedida");
        result.Data.Should().BeEquivalentTo(updatedUserDto, "os dados retornados devem corresponder ao usuário atualizado");

        // Verify
        await userRepositoryMock.Received(1).GetByIdAsync(userId);
        await userRepositoryMock.Received(1).UpdateAsync(existingUser);
        await unitOfWorkMock.Received(1).BeginTransactionAsync();
        await unitOfWorkMock.Received(1).CommitAsync();
        await validatorMock.Received(1).ValidateAsync(requestUserDto);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 999;
        var requestUserDto = new RequestUserDto
        {
            Email = "nonexistent@example.com",
            Username = "nonexistentuser",
            Password = "SomePassword123",
            Name = new NameDto
            {
                Firstname = "Nonexistent",
                Lastname = "User"
            },
            Address = new AddressDto
            {
                City = "Nowhere City",
                Street = "Unknown Street",
                Number = 0,
                Zipcode = "00000-000",
                Geolocation = new GeolocationDto
                {
                    Lat = "0.0",
                    Long = "0.0"
                }
            },
            Phone = "000000000",
            Status = "Inactive",
            Role = "Customer"
        };

        var expectedMessage = $"Usuário com o ID {userId} não encontrado.";

        var userRepositoryMock = Substitute.For<IUserRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();
        var mapperMock = Substitute.For<IMapper>();
        var validatorMock = Substitute.For<IValidator<RequestUserDto>>();

        var validationResult = new FluentValidation.Results.ValidationResult();
        validatorMock.ValidateAsync(requestUserDto, Arg.Any<CancellationToken>())
                     .Returns(validationResult);

        userRepositoryMock.GetByIdAsync(userId).Returns((User?)null);

        var userService = new UserService(userRepositoryMock, mapperMock, validatorMock, unitOfWorkMock);

        // Act
        var result = await userService.UpdateAsync(userId, requestUserDto);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeFalse("a operação deve falhar quando o usuário não existe");
        result.StatusCode.Should().Be(404, "o status HTTP deve ser 404 para um recurso não encontrado");
        result.Message.Should().Be(expectedMessage, "a mensagem deve indicar que o usuário não foi encontrado");
        result.Data.Should().BeNull("os dados devem ser nulos quando o usuário não existe");

        // Verify
        await userRepositoryMock.Received(1).GetByIdAsync(userId);
        await userRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<User>());
        await unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await unitOfWorkMock.DidNotReceive().CommitAsync();
        await validatorMock.Received(1).ValidateAsync(requestUserDto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var requestUserDto = new RequestUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "password123",
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

        var user = new User
        {
            Id = 1,
            Email = requestUserDto.Email,
            Username = requestUserDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestUserDto.Password),
            Firstname = requestUserDto.Name.Firstname,
            Lastname = requestUserDto.Name.Lastname,
            City = requestUserDto.Address.City,
            Street = requestUserDto.Address.Street,
            AddressNumber = requestUserDto.Address.Number,
            Zipcode = requestUserDto.Address.Zipcode,
            GeolocationLat = requestUserDto.Address.Geolocation.Lat,
            GeolocationLong = requestUserDto.Address.Geolocation.Long,
            Phone = requestUserDto.Phone,
            Status = requestUserDto.Status,
            Role = requestUserDto.Role
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

        // Configurar mocks
        userRepositoryMock.ExistsByEmailAsync(requestUserDto.Email).Returns(false);
        mapperMock.Map<User>(requestUserDto).Returns(user);
        mapperMock.Map<UserDto>(user).Returns(userDto);
        validatorMock.ValidateAsync(requestUserDto, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var userService = new UserService(userRepositoryMock, mapperMock, validatorMock, unitOfWorkMock);

        // Act
        var result = await userService.CreateAsync(requestUserDto);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeTrue("a operação deve ser bem-sucedida para um usuário válido");
        result.StatusCode.Should().Be(201, "o status HTTP deve ser 201 para uma criação bem-sucedida");
        result.Data.Should().BeEquivalentTo(userDto, "os dados retornados devem corresponder ao usuário criado");

        // Verify
        await userRepositoryMock.Received(1).ExistsByEmailAsync(requestUserDto.Email);
        await userRepositoryMock.Received(1).AddAsync(user);
        await unitOfWorkMock.Received(1).BeginTransactionAsync();
        await unitOfWorkMock.Received(1).CommitAsync();
        await validatorMock.Received(1).ValidateAsync(requestUserDto, Arg.Any<CancellationToken>());
    }
}
