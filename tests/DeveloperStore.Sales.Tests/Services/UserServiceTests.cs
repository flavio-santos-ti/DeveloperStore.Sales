using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class UserServiceTests
{
    private readonly IMapper _mapperMock;
    private readonly IValidator<RequestUserDto> _validatorMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mapperMock = Substitute.For<IMapper>();
        _validatorMock = Substitute.For<IValidator<RequestUserDto>>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userService = new UserService(_mapperMock, _validatorMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var requestDto = new RequestUserDto
        {
            Email = "validemail@example.com",
            Username = "validuser",
            Password = "StrongPassword123!",
            Name = new NameDto { Firstname = "John", Lastname = "Doe" },
            Address = new AddressDto
            {
                City = "New York",
                Street = "Main Street",
                Number = 123,
                Zipcode = "10001",
                Geolocation = new GeolocationDto { Lat = "40.7128", Long = "-74.0060" }
            },
            Phone = "123456789",
            Status = "Active",
            Role = "Customer"
        };

        var validationResult = new FluentValidation.Results.ValidationResult(); // Valid result

        _validatorMock.ValidateAsync(requestDto, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(validationResult));

        var user = new User
        {
            Id = 1,
            Email = requestDto.Email,
            Username = requestDto.Username,
            Firstname = requestDto.Name.Firstname,
            Lastname = requestDto.Name.Lastname,
            City = requestDto.Address.City,
            Street = requestDto.Address.Street,
            AddressNumber = requestDto.Address.Number,
            Zipcode = requestDto.Address.Zipcode,
            GeolocationLat = requestDto.Address.Geolocation.Lat,
            GeolocationLong = requestDto.Address.Geolocation.Long,
            Phone = requestDto.Phone,
            Status = requestDto.Status,
            Role = requestDto.Role,
            PasswordHash = "hashedpassword"
        };

        _mapperMock.Map<User>(requestDto).Returns(user);
        _mapperMock.Map<UserDto>(user).Returns(new UserDto
        {
            Email = user.Email,
            Username = user.Username,
            Name = new NameDto { Firstname = user.Firstname, Lastname = user.Lastname },
            Address = new AddressDto
            {
                City = user.City,
                Street = user.Street,
                Number = user.AddressNumber,
                Zipcode = user.Zipcode,
                Geolocation = new GeolocationDto { Lat = user.GeolocationLat, Long = user.GeolocationLong }
            },
            Phone = user.Phone,
            Status = user.Status,
            Role = user.Role
        });

        _unitOfWorkMock.UserRepository.ExistsByEmailAsync(requestDto.Email)
            .Returns(Task.FromResult(false)); // Email does not exist

        // Act
        var result = await _userService.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(requestDto.Email);

        // Verify
        await _validatorMock.Received(1).ValidateAsync(requestDto, Arg.Any<CancellationToken>());

        await _unitOfWorkMock.UserRepository.Received(1).AddAsync(Arg.Any<User>());
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenEmailExists()
    {
        // Arrange
        var requestDto = new RequestUserDto
        {
            Email = "existingemail@example.com",
            Username = "existinguser",
            Password = "StrongPassword123!",
            Name = new NameDto { Firstname = "John", Lastname = "Doe" },
            Address = new AddressDto
            {
                City = "New York",
                Street = "Main Street",
                Number = 123,
                Zipcode = "10001",
                Geolocation = new GeolocationDto { Lat = "40.7128", Long = "-74.0060" }
            },
            Phone = "123456789",
            Status = "Active",
            Role = "Customer"
        };

        var validationResult = new FluentValidation.Results.ValidationResult(); // Valid result

        _validatorMock.ValidateAsync(requestDto, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(validationResult));

        _unitOfWorkMock.UserRepository.ExistsByEmailAsync(requestDto.Email)
            .Returns(Task.FromResult(true)); // Email exists

        // Act
        var result = await _userService.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Já existe um usuário com este e-mail.");

        // Verify
        await _validatorMock.Received(1).ValidateAsync(requestDto, Arg.Any<CancellationToken>());

        await _unitOfWorkMock.UserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var requestDto = new RequestUserDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "password123"
        };

        var existingUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            Username = "olduser"
        };

        _unitOfWorkMock.UserRepository.GetByIdAsync(existingUser.Id).Returns(existingUser);
        _mapperMock.Map(requestDto, existingUser).Returns(existingUser);
        _validatorMock.ValidateAsync(requestDto, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        // Act
        var result = await _userService.UpdateAsync(existingUser.Id, requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        // Verify
        await _unitOfWorkMock.UserRepository.Received(1).UpdateAsync(existingUser);
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var existingUser = new User { Id = 1, Email = "test@example.com" };
        _unitOfWorkMock.UserRepository.GetByIdAsync(existingUser.Id).Returns(existingUser);

        // Act
        var result = await _userService.DeleteAsync(existingUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        // Verify
        await _unitOfWorkMock.UserRepository.Received(1).DeleteAsync(existingUser);
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.Received(1).CommitAsync();
    }
}
