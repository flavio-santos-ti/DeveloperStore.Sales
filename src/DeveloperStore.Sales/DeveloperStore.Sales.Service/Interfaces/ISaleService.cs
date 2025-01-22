using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.Sale;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface ISaleService
{
    Task<ApiResponseDto<SaleDto>> CreateAsync(RequestSaleDto dto);
    Task<ApiResponseDto<string>> CancelSaleAsync(int saleId);
    Task<ApiResponseDto<string>> UpdateSaleAsync(int saleId, RequestSaleDto dto);
    Task<ApiResponseDto<string>> CancelSaleItemAsync(int saleId, int itemId);
    Task<ApiResponseDto<SaleDto>> CheckoutCartAsync(int cartId);
}
