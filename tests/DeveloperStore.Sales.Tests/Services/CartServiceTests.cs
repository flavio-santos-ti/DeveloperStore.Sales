using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.UnitOfWork;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class CartServiceTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IMapper _mapperMock;
    private readonly IValidator<RequestCartDto> _validatorMock;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _mapperMock = Substitute.For<IMapper>();
        _validatorMock = Substitute.For<IValidator<RequestCartDto>>();

        _cartService = new CartService(_unitOfWorkMock, _mapperMock, _validatorMock);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenCartIsValid()
    {
        // Arrange
        var cartDto = new RequestCartDto
        {
            UserId = 1,
            Date = DateTime.UtcNow,
            Products = new List<RequestCartProductDto>
            {
                new RequestCartProductDto { ProductId = 1, Quantity = 2 }
            }
        };

        var validationResult = new ValidationResult();
        _validatorMock.ValidateAsync(cartDto).Returns(validationResult);

        var cart = new Cart { Id = 1, UserId = cartDto.UserId, Date = cartDto.Date };
        _unitOfWorkMock.CartRepository.AddAsync(Arg.Any<Cart>()).Returns(Task.CompletedTask);

        _unitOfWorkMock.CartProductRepository.AddAsync(Arg.Any<CartProduct>()).Returns(Task.CompletedTask);

        _unitOfWorkMock.CommitAsync().Returns(Task.CompletedTask);
        _mapperMock.Map<CartDto>(Arg.Any<Cart>()).Returns(new CartDto { Id = cart.Id, UserId = cart.UserId, Date = cart.Date });

        // Act
        var result = await _cartService.CreateAsync(cartDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(cart.Id);
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenCartExists()
    {
        // Arrange
        var cartId = 1;
        var cart = new Cart { Id = cartId };

        _unitOfWorkMock.CartRepository.GetByIdAsync(cartId).Returns(cart);
        _unitOfWorkMock.CartRepository.DeleteAsync(cart).Returns(Task.CompletedTask);
        _unitOfWorkMock.CommitAsync().Returns(Task.CompletedTask);

        // Act
        var result = await _cartService.DeleteAsync(cartId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("excluído com sucesso");
        await _unitOfWorkMock.CartRepository.Received(1).DeleteAsync(cart);
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        var cartId = 1;
        _unitOfWorkMock.CartRepository.GetByIdAsync(cartId).Returns((Cart?)null);

        // Act
        var result = await _cartService.DeleteAsync(cartId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("não encontrado");
        await _unitOfWorkMock.CartRepository.Received(1).GetByIdAsync(cartId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        var cartId = 99;
        var requestDto = new RequestCartDto
        {
            UserId = 1,
            Date = DateTime.UtcNow,
            Products = new List<RequestCartProductDto>
            {
                new RequestCartProductDto { ProductId = 1, Quantity = 2 }
            }
        };

        var validationResult = new FluentValidation.Results.ValidationResult();
        _validatorMock.ValidateAsync(requestDto, default).Returns(validationResult);
        _unitOfWorkMock.CartRepository.GetByIdAsync(cartId).Returns(Task.FromResult<Cart?>(null));

        // Act
        var result = await _cartService.UpdateAsync(cartId, requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be($"Carrinho com ID {cartId} não encontrado.");

        await _validatorMock.Received(1).ValidateAsync(requestDto, default);

        await _unitOfWorkMock.CartRepository.Received(1).GetByIdAsync(cartId);

        await _unitOfWorkMock.CartProductRepository.DidNotReceiveWithAnyArgs().GetByCartIdAsync(Arg.Any<int>());
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().BeginTransactionAsync();
        await _unitOfWorkMock.DidNotReceiveWithAnyArgs().CommitAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenCartIsValid()
    {
        // Arrange
        var cartId = 1;
        var requestDto = new RequestCartDto
        {
            UserId = 2,
            Date = DateTime.UtcNow,
            Products = new List<RequestCartProductDto>
        {
            new RequestCartProductDto { ProductId = 1, Quantity = 3 }, // Produto existente com quantidade alterada
            new RequestCartProductDto { ProductId = 3, Quantity = 5 }  // Novo produto
        }
        };

        var existingProducts = new List<CartProduct>
    {
        new CartProduct { ProductId = 1, Quantity = 2, CartId = cartId },
        new CartProduct { ProductId = 2, Quantity = 1, CartId = cartId }
    };

        var existingCart = new Cart
        {
            Id = cartId,
            UserId = 1,
            Date = DateTime.UtcNow,
            CartProducts = existingProducts
        };

        _unitOfWorkMock.CartRepository.GetByIdAsync(cartId).Returns(existingCart);
        _unitOfWorkMock.CartProductRepository.GetByCartIdAsync(cartId).Returns(existingProducts);
        _validatorMock.ValidateAsync(requestDto, default).Returns(new ValidationResult());

        // Act
        var result = await _cartService.UpdateAsync(cartId, requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        await _unitOfWorkMock.CartProductRepository.Received(1)
            .DeleteAsync(Arg.Is<CartProduct>(cp => cp.ProductId == 2 && cp.CartId == cartId));
        await _unitOfWorkMock.CartProductRepository.Received(1)
            .AddAsync(Arg.Is<CartProduct>(cp => cp.ProductId == 3 && cp.Quantity == 5 && cp.CartId == cartId));

        await _unitOfWorkMock.CartRepository.Received(1)
            .UpdateAsync(Arg.Is<Cart>(c =>
                c.CartProducts.Count == 2 &&
                c.CartProducts.Any(cp => cp.ProductId == 1 && cp.Quantity == 3) &&
                c.CartProducts.Any(cp => cp.ProductId == 3 && cp.Quantity == 5)
            ));

        await _unitOfWorkMock.Received(1).CommitAsync();
    }
}
