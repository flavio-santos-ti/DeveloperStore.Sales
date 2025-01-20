using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Dtos.Response;

namespace DeveloperStore.Sales.Service.Interfaces;

public interface IProductService
{
    Task<ApiResponseDto<ProductDto>> CreateAsync(RequestProductDto dto);
    Task<ApiResponseDto<ProductDto>> UpdateAsync(int id, RequestProductDto dto);
    Task<ApiResponseDto<ProductDto>> DeleteAsync(int id);
    Task<ApiResponseDto<PagedResponseDto<ProductDto>>> GetAllAsync(int page = 1, int size = 10, string? order = null);
    Task<ApiResponseDto<PagedResponseDto<ProductDto>>> GetByCategoryAsync(string category, int page = 1, int size = 10, string? order = null);
    Task<ApiResponseDto<ProductDto>> GetByIdAsync(int id);
}
