using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Extensions;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.Extensions;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Service.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<RequestCartDto> _validator;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<RequestCartDto> validator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ApiResponseDto<CartDto>> CreateAsync(RequestCartDto dto)
    {
        if (dto == null)
            return ApiResponseDto<CartDto>.AsBadRequest("Os dados do carrinho não podem ser nulos.");

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return validationResult.ToApiResponse<CartDto>();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = new Cart
            {
                UserId = dto.UserId,
                Date = dto.Date,
            };

            await _unitOfWork.CartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync(); 
            
            foreach (var product in dto.Products)
            {
                var cartProduct = new CartProduct
                {
                    CartId = cart.Id, 
                    ProductId = product.ProductId,
                    Quantity = product.Quantity
                };

                await _unitOfWork.CartProductRepository.AddAsync(cartProduct);
            }

            await _unitOfWork.CommitAsync();
            
            var cartDto = _mapper.Map<CartDto>(cart);
            return ApiResponseDto<CartDto>.AsCreated(cartDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<CartDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<CartDto>> UpdateAsync(int id, RequestCartDto dto)
    {
        if (dto == null)
            return ApiResponseDto<CartDto>.AsBadRequest("Os dados do carrinho não podem ser nulos.");

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return validationResult.ToApiResponse<CartDto>();

        var existingCart = await _unitOfWork.CartRepository.GetByIdAsync(id);
        if (existingCart == null)
            return ApiResponseDto<CartDto>.AsNotFound($"Carrinho com ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            existingCart.UserId = dto.UserId;
            existingCart.Date = dto.Date;

            var existingProducts = await _unitOfWork.CartProductRepository.GetByCartIdAsync(id);

            var productsToRemove = existingProducts.Where(ep => !dto.Products.Any(dp => dp.ProductId == ep.ProductId)).ToList();
            foreach (var product in productsToRemove)
            {
                await _unitOfWork.CartProductRepository.DeleteAsync(product);
            }

            foreach (var product in dto.Products)
            {
                var existingProduct = existingProducts.FirstOrDefault(ep => ep.ProductId == product.ProductId);
                if (existingProduct != null)
                {
                    existingProduct.Quantity = product.Quantity;
                    await _unitOfWork.CartProductRepository.UpdateAsync(existingProduct);
                }
                else
                {
                    var newCartProduct = new CartProduct
                    {
                        CartId = id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity
                    };
                    await _unitOfWork.CartProductRepository.AddAsync(newCartProduct);
                }
            }

            // Atualiza o estado do CartProducts
            existingCart.CartProducts = existingProducts
                .Where(ep => !productsToRemove.Any(p => p.ProductId == ep.ProductId))
                .Concat(dto.Products
                    .Where(dp => !existingProducts.Any(ep => ep.ProductId == dp.ProductId))
                    .Select(dp => new CartProduct
                    {
                        ProductId = dp.ProductId,
                        Quantity = dp.Quantity,
                        CartId = id
                    }))
                .ToList();

            // Atualiza as quantidades dos produtos existentes
            foreach (var product in dto.Products)
            {
                var existingProduct = existingCart.CartProducts.FirstOrDefault(cp => cp.ProductId == product.ProductId);
                if (existingProduct != null)
                {
                    existingProduct.Quantity = product.Quantity;
                }
            }

            await _unitOfWork.CartRepository.UpdateAsync(existingCart);
            await _unitOfWork.CommitAsync();

            var cartDto = _mapper.Map<CartDto>(existingCart);
            return ApiResponseDto<CartDto>.AsSuccess(cartDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<CartDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<string>> DeleteAsync(int id)
    {
        var existingCart = await _unitOfWork.CartRepository.GetByIdAsync(id);
        if (existingCart == null)
            return ApiResponseDto<string>.AsNotFound($"Carrinho com ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Excluindo o carrinho
            await _unitOfWork.CartRepository.DeleteAsync(existingCart);
            await _unitOfWork.CommitAsync();

            return ApiResponseDto<string>.AsSuccess($"Carrinho com ID {id} excluído com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<string>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<CartDto>> GetByIdAsync(int id)
    {
        var existingCart = await _unitOfWork.CartRepository.GetByIdAsync(id);
        if (existingCart == null)
            return ApiResponseDto<CartDto>.AsNotFound($"Carrinho com ID {id} não encontrado.");

        var products = await _unitOfWork.CartProductRepository.GetByCartIdAsync(id);
        var cartDto = _mapper.Map<CartDto>(existingCart);

        cartDto.Products = products.Select(p => new CartProductDto
        {
            ProductId = p.ProductId,
            Quantity = p.Quantity
        }).ToList();

        return ApiResponseDto<CartDto>.AsSuccess(cartDto);
    }

    public async Task<ApiResponseDto<PagedResponseDto<CartDto>>> GetAllAsync(int page = 1, int size = 10, string? order = null)
    {
        try
        {
            var query = _unitOfWork.CartRepository.GetAllQueryable();

            // Aplicar ordenação dinâmica
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

                    var propertyInfo = typeof(Cart).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (propertyInfo == null)
                    {
                        return ApiResponseDto<PagedResponseDto<CartDto>>.AsBadRequest(
                            $"Propriedade '{propertyName}' não encontrada no modelo.");
                    }

                    query = isFirstOrder
                        ? query.OrderByDynamic(propertyInfo.Name, isDescending)
                        : query.ThenByDynamic(propertyInfo.Name, isDescending);

                    isFirstOrder = false;
                }
            }

            var totalItems = await query.CountAsync();
            var carts = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            if (!carts.Any())
                return ApiResponseDto<PagedResponseDto<CartDto>>.AsNotFound("Nenhum carrinho encontrado.");

            var cartDtos = carts.Select(cart => new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Date = cart.Date,
                Products = cart.CartProducts.Select(cp => new CartProductDto
                {
                    ProductId = cp.ProductId,
                    Quantity = cp.Quantity
                }).ToList()
            }).ToList();

            var pagedResponse = new PagedResponseDto<CartDto>(
                data: cartDtos,
                totalItems: totalItems,
                currentPage: page,
                totalPages: (int)Math.Ceiling((double)totalItems / size)
            );

            return ApiResponseDto<PagedResponseDto<CartDto>>.AsSuccess(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PagedResponseDto<CartDto>>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

}
