using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface IUserService
{
    Task<ApiResponseDto<UserDto>> CreateAsync(RequestUserDto dto);
    Task<ApiResponseDto<UserDto>> UpdateAsync(int id, RequestUserDto dto);
    Task<ApiResponseDto<UserDto>> DeleteAsync(int id);
}
