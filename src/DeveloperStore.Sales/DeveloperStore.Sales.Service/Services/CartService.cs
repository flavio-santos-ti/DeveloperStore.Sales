using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Extensions;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentValidation;

namespace DeveloperStore.Sales.Service.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<RequestCartDto> _validator;
    private readonly ICartProductRepository _cartProductRepository;

    public CartService(ICartRepository cartRepository, IUnitOfWork unitOfWork, IMapper mapper, IValidator<RequestCartDto> validator, ICartProductRepository cartProductRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _cartProductRepository = cartProductRepository ?? throw new ArgumentNullException(nameof(cartProductRepository));
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

            await _cartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync(); 
            
            foreach (var product in dto.Products)
            {
                var cartProduct = new CartProduct
                {
                    CartId = cart.Id, 
                    ProductId = product.ProductId,
                    Quantity = product.Quantity
                };

                await _cartProductRepository.AddAsync(cartProduct);
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
}
