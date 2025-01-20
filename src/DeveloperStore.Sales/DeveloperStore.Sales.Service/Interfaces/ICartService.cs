using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Dtos.Response;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface ICartService
{
    Task<ApiResponseDto<CartDto>> CreateAsync(RequestCartDto dto);
    Task<ApiResponseDto<CartDto>> UpdateAsync(int id, RequestCartDto dto);
    Task<ApiResponseDto<string>> DeleteAsync(int id);
    Task<ApiResponseDto<CartDto>> GetByIdAsync(int id);
}
