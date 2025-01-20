using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Extensions;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.Extensions;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Service.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RequestProductDto> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IMapper mapper, IValidator<RequestProductDto> validator, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseDto<ProductDto>> CreateAsync(RequestProductDto dto)
    {
        if (dto == null)
            return ApiResponseDto<ProductDto>.AsBadRequest("Os dados do produto não podem ser nulos.");

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return validationResult.ToApiResponse<ProductDto>();

        var existingProduct = await _productRepository.ExistsByTitleAsync(dto.Title);
        if (existingProduct)
            return ApiResponseDto<ProductDto>.AsBadRequest("Já existe um produto com o mesmo título.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var product = _mapper.Map<Product>(dto);

            await _productRepository.AddAsync(product);

            await _unitOfWork.CommitAsync();

            var productDto = _mapper.Map<ProductDto>(product);

            return ApiResponseDto<ProductDto>.AsCreated(productDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<ProductDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> UpdateAsync(int id, RequestProductDto dto)
    {
        if (dto == null)
            return ApiResponseDto<ProductDto>.AsBadRequest("Os dados do produto não podem ser nulos.");

        var valResult = await _validator.ValidateAsync(dto);
        if (!valResult.IsValid)
        {
            return valResult.ToApiResponse<ProductDto>();
        }

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            return ApiResponseDto<ProductDto>.AsNotFound($"Produto com o ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _mapper.Map(dto, existingProduct);
            await _productRepository.UpdateAsync(existingProduct);

            await _unitOfWork.CommitAsync();

            var updatedProductDto = _mapper.Map<ProductDto>(existingProduct);

            return ApiResponseDto<ProductDto>.AsSuccess(updatedProductDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<ProductDto>
                .AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> DeleteAsync(int id)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            return ApiResponseDto<ProductDto>.AsNotFound($"Produto com o ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _productRepository.DeleteAsync(existingProduct);

            await _unitOfWork.CommitAsync();

            return ApiResponseDto<ProductDto>.AsSuccess($"Produto com o ID {id} excluído com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<ProductDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PagedResponseDto<ProductDto>>> GetAllAsync(int page = 1, int size = 10, string? order = null)
    {
        try
        {
            var query = _productRepository.GetAllQueryable().AsNoTracking();
            
            // Ordenação dinâmica
            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderParams = order.Split(',');
                bool isFirstOrder = true;

                foreach (var param in orderParams)
                {
                    var isDescending = param.Trim().EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
                    var propertyName = isDescending
                        ? param.Replace(" desc", "", StringComparison.OrdinalIgnoreCase).Trim()
                        : param.Replace(" asc", "", StringComparison.OrdinalIgnoreCase).Trim();

                    var propertyInfo = typeof(Product).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (propertyInfo == null)
                    {
                        return ApiResponseDto<PagedResponseDto<ProductDto>>.AsBadRequest(
                            $"Propriedade '{propertyName}' não encontrada no modelo.");
                    }

                    query = isFirstOrder
                        ? query.OrderByDynamic(propertyInfo.Name, isDescending)
                        : query.ThenByDynamic(propertyInfo.Name, isDescending);

                    isFirstOrder = false;
                }
            }

            // Paginação
            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            if (!products.Any())
                return ApiResponseDto<PagedResponseDto<ProductDto>>.AsNotFound("Nenhum produto encontrado.");

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            var pagedResponse = new PagedResponseDto<ProductDto>(
                data: productDtos!,
                totalItems: totalItems,
                currentPage: page,
                totalPages: (int)Math.Ceiling((double)totalItems / size)
            );

            return ApiResponseDto<PagedResponseDto<ProductDto>>.AsSuccess(pagedResponse);

        }
        catch (Exception ex)
        {
            return ApiResponseDto<PagedResponseDto<ProductDto>>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PagedResponseDto<ProductDto>>> GetByCategoryAsync(
        string category,
        int page = 1,
        int size = 10,
        string? order = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
                return ApiResponseDto<PagedResponseDto<ProductDto>>.AsBadRequest("A categoria não pode ser nula ou vazia.");

            // Convertendo a consulta para memória antes da comparação
            var query = _productRepository.GetAllQueryable()
                .Where(p => p.Category.ToLower() == category.ToLower());

            // Ordenação dinâmica
            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderParams = order.Split(',');
                bool isFirstOrder = true;

                foreach (var param in orderParams)
                {
                    var isDescending = param.Trim().EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
                    var propertyName = isDescending
                        ? param.Replace(" desc", "", StringComparison.OrdinalIgnoreCase).Trim()
                        : param.Replace(" asc", "", StringComparison.OrdinalIgnoreCase).Trim();

                    var propertyInfo = typeof(Product).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (propertyInfo == null)
                    {
                        return ApiResponseDto<PagedResponseDto<ProductDto>>.AsBadRequest(
                            $"Propriedade '{propertyName}' não encontrada no modelo.");
                    }

                    query = isFirstOrder
                        ? query.OrderByDynamic(propertyInfo.Name, isDescending)
                        : query.ThenByDynamic(propertyInfo.Name, isDescending);

                    isFirstOrder = false;
                }
            }

            // Paginação
            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            if (!products.Any())
                return ApiResponseDto<PagedResponseDto<ProductDto>>.AsNotFound($"Nenhum produto encontrado na categoria '{category}'.");

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            var pagedResponse = new PagedResponseDto<ProductDto>(
                data: productDtos,
                totalItems: totalItems,
                currentPage: page,
                totalPages: (int)Math.Ceiling((double)totalItems / size)
            );

            return ApiResponseDto<PagedResponseDto<ProductDto>>.AsSuccess(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PagedResponseDto<ProductDto>>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return ApiResponseDto<ProductDto>.AsBadRequest("O ID do produto deve ser maior que zero.");

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return ApiResponseDto<ProductDto>.AsNotFound($"Produto com o ID {id} não encontrado.");

            var productDto = _mapper.Map<ProductDto>(product);

            return ApiResponseDto<ProductDto>.AsSuccess(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }


}
