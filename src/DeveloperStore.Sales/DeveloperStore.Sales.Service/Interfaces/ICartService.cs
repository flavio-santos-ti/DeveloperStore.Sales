using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Dtos.Response;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface ICartService
{
    Task<ApiResponseDto<CartDto>> CreateAsync(RequestCartDto dto);
}
