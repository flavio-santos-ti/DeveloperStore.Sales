using DeveloperStore.Sales.Domain.Dtos.Auth;
using DeveloperStore.Sales.Domain.Dtos.Response;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> AuthenticateAsync(AuthRequestDto dto);
}
