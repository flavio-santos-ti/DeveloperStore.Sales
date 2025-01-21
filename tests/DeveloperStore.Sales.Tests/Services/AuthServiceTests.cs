using DeveloperStore.Sales.Domain.Dtos.Auth;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Configuration;

namespace DeveloperStore.Sales.Tests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", new string('a', 32) }, 
            { "Jwt:Issuer", "testissuer" },
            { "Jwt:Audience", "testaudience" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(_userRepositoryMock, _configuration);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _authService.AuthenticateAsync(null);

        // Assert
        result.Should().NotBeNull("a resposta não deve ser nula");
        result.IsSuccess.Should().BeFalse("a autenticação deve falhar com um request nulo");
        result.StatusCode.Should().Be(400, "o status HTTP deve ser 400 para uma requisição inválida");
        result.Data.Should().BeNull("nenhum dado deve ser retornado em caso de falha");
        result.Message.Should().Be("Dados de autenticação não podem ser nulos.", "a mensagem deve indicar erro de validação");

        // Verify
        await _userRepositoryMock.DidNotReceive().GetByUsernameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var requestDto = new AuthRequestDto
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };

        _userRepositoryMock.GetByUsernameAsync(requestDto.Username).Returns((User?)null);

        // Act
        var result = await _authService.AuthenticateAsync(requestDto);

        // Assert
        result.Should().NotBeNull("a resposta não deve ser nula");
        result.IsSuccess.Should().BeFalse("a autenticação deve falhar com credenciais inválidas");
        result.StatusCode.Should().Be(401, "o status HTTP deve ser 401 para credenciais inválidas");
        result.Data.Should().BeNull("nenhum dado deve ser retornado em caso de falha");
        result.Message.Should().Be("Usuário ou senha inválidos.", "a mensagem deve indicar erro de autenticação");

        // Verify
        await _userRepositoryMock.Received(1).GetByUsernameAsync(requestDto.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldThrowException_WhenJwtSecretIsInvalid()
    {
        // Arrange
        var invalidConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
        { "Jwt:Secret", "shortkey" }, 
            })
            .Build();

        var authServiceWithInvalidConfig = new AuthService(_userRepositoryMock, invalidConfiguration);

        var requestDto = new AuthRequestDto
        {
            Username = "testuser",
            Password = "password123"
        };

        var user = new User
        {
            Username = requestDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.Password),
            Role = "User"
        };

        _userRepositoryMock.GetByUsernameAsync(requestDto.Username).Returns(user);

        // Act
        Func<Task> act = async () => await authServiceWithInvalidConfig.AuthenticateAsync(requestDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A chave JWT deve ter pelo menos 32 caracteres.");

        // Verify
        await _userRepositoryMock.Received(1).GetByUsernameAsync(requestDto.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnToken_WhenRequestIsValid()
    {
        // Arrange
        var requestDto = new AuthRequestDto
        {
            Username = "testuser",
            Password = "password123"
        };

        var user = new User
        {
            Username = requestDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.Password),
            Role = "User"
        };

        _userRepositoryMock.GetByUsernameAsync(requestDto.Username).Returns(user);

        // Act
        var result = await _authService.AuthenticateAsync(requestDto);

        // Assert
        result.Should().NotBeNull("a resposta não deve ser nula");
        result.IsSuccess.Should().BeTrue("a autenticação deve ser bem-sucedida");
        result.StatusCode.Should().Be(200, "o status HTTP deve ser 200 para uma autenticação bem-sucedida");
        result.Data.Should().NotBeNull("o token deve ser retornado em caso de sucesso");
        result.Data.Token.Should().NotBeNullOrWhiteSpace("o token JWT deve ser gerado");

        // Verify
        await _userRepositoryMock.Received(1).GetByUsernameAsync(requestDto.Username);
    }
}
