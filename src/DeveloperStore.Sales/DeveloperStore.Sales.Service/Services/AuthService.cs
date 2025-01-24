using DeveloperStore.Sales.Domain.Dtos.Auth;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.UnitOfWork;

namespace DeveloperStore.Sales.Service.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseDto<AuthResponseDto>> AuthenticateAsync(AuthRequestDto dto)
    {
        if (dto == null)
            return ApiResponseDto<AuthResponseDto>.AsBadRequest("Dados de autenticação não podem ser nulos.");

        var user = await _unitOfWork.UserRepository.GetByUsernameAsync(dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponseDto<AuthResponseDto>.AsUnauthorized("Usuário ou senha inválidos.");

        var token = GenerateJwtToken(user);

        return ApiResponseDto<AuthResponseDto>.AsSuccess(new AuthResponseDto { Token = token });
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            throw new ArgumentException("A chave JWT deve ter pelo menos 32 caracteres.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
