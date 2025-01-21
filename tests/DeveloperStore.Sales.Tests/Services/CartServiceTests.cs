using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using DeveloperStore.Sales.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class CartServiceTests
{
    private readonly ICartRepository _cartRepositoryMock;
    private readonly ICartProductRepository _cartProductRepositoryMock;
    private readonly IMapper _mapperMock;
    private readonly CartService _cartService;
    private readonly IValidator<RequestCartDto> _validatorMock;

    public CartServiceTests()
    {
        _cartRepositoryMock = Substitute.For<ICartRepository>();
        _cartProductRepositoryMock = Substitute.For<ICartProductRepository>();
        _mapperMock = Substitute.For<IMapper>();
        _validatorMock = Substitute.For<IValidator<RequestCartDto>>(); // Inicializa o mock do validador

        // Configuração do mock do validador
        var validationResultMock = new FluentValidation.Results.ValidationResult();
        _validatorMock.ValidateAsync(Arg.Any<RequestCartDto>()).Returns(validationResultMock);

        // Passa o mock configurado no construtor do CartService
        _cartService = new CartService(
            _cartRepositoryMock,
            Substitute.For<IUnitOfWork>(),
            _mapperMock,
            _validatorMock, // Passa o mock do validador configurado aqui
            _cartProductRepositoryMock
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCart_WhenCartExists()
    {
        // Arrange
        int cartId = 1;

        var cart = new Cart
        {
            Id = cartId,
            UserId = 123,
            Date = DateTime.Now
        };

        var cartProducts = new List<CartProduct>
        {
            new CartProduct { CartId = cartId, ProductId = 101, Quantity = 2 },
            new CartProduct { CartId = cartId, ProductId = 102, Quantity = 3 }
        };

        var expectedCartDto = new CartDto
        {
            Id = cartId,
            UserId = 123,
            Date = cart.Date,
            Products = cartProducts.Select(p => new CartProductDto
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            }).ToList()
        };

        _cartRepositoryMock.GetByIdAsync(cartId).Returns(cart);
        _cartProductRepositoryMock.GetByCartIdAsync(cartId).Returns(cartProducts);
        _mapperMock.Map<CartDto>(cart).Returns(expectedCartDto);

        // Act
        var response = await _cartService.GetByIdAsync(cartId);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data.Should().BeEquivalentTo(expectedCartDto);

        await _cartRepositoryMock.Received(1).GetByIdAsync(cartId);
        await _cartProductRepositoryMock.Received(1).GetByCartIdAsync(cartId);
        _mapperMock.Received(1).Map<CartDto>(cart);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        int cartId = 1;

        _cartRepositoryMock.GetByIdAsync(cartId).Returns((Cart?)null);

        // Act
        var response = await _cartService.GetByIdAsync(cartId);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Data.Should().BeNull();
        response.Message.Should().Be($"Carrinho com ID {cartId} não encontrado.");

        await _cartRepositoryMock.Received(1).GetByIdAsync(cartId);
        await _cartProductRepositoryMock.DidNotReceive().GetByCartIdAsync(cartId);
        _mapperMock.DidNotReceive().Map<CartDto>(Arg.Any<Cart>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResponse_WhenCartsExist()
    {
        // Arrange
        var mockCarts = new List<Cart>
        {
            new Cart { Id = 1, UserId = 123, Date = DateTime.Now },
            new Cart { Id = 2, UserId = 456, Date = DateTime.Now }
        };

        var mockCartsDto = mockCarts.Select(cart => new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Date = cart.Date,
            Products = new List<CartProductDto>() // Simule os produtos, se necessário
        }).ToList();

        var mockQueryable = new TestAsyncEnumerable<Cart>(mockCarts);

        _cartRepositoryMock.GetAllQueryable().Returns(mockQueryable);

        _mapperMock
            .Map<IEnumerable<CartDto>>(Arg.Any<IEnumerable<Cart>>())
            .Returns(mockCartsDto);

        // Act
        var response = await _cartService.GetAllAsync(1, 10, null);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data.Data.Count().Should().Be(mockCartsDto.Count);
        response.Data.CurrentPage.Should().Be(1);
        response.Data.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNotFound_WhenNoCartsExist()
    {
        // Arrange
        var emptyCarts = new TestAsyncEnumerable<Cart>(new List<Cart>());

        _cartRepositoryMock.GetAllQueryable().Returns(emptyCarts);

        // Act
        var response = await _cartService.GetAllAsync(1, 10, null);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be("Nenhum carrinho encontrado.");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenCartExists()
    {
        // Arrange
        int cartId = 1;

        var existingCart = new Cart
        {
            Id = cartId,
            UserId = 123,
            Date = DateTime.Now
        };

        var cartRepositoryMock = Substitute.For<ICartRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();

        cartRepositoryMock.GetByIdAsync(cartId).Returns(existingCart);

        var cartService = new CartService(
            cartRepositoryMock,
            unitOfWorkMock,
            Substitute.For<IMapper>(),
            Substitute.For<FluentValidation.IValidator<RequestCartDto>>(),
            Substitute.For<ICartProductRepository>()
        );

        // Act
        var result = await cartService.DeleteAsync(cartId);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeTrue("a operação deve ser bem-sucedida quando o carrinho existe");
        result.StatusCode.Should().Be(200, "o status HTTP deve ser 200 para uma exclusão bem-sucedida");
        result.Message.Should().Be($"Carrinho com ID {cartId} excluído com sucesso.", "a mensagem deve indicar que o carrinho foi excluído com sucesso");
        result.Data.Should().BeNull("os dados retornados devem ser nulos, já que apenas uma mensagem de sucesso é esperada");

        // Verify
        await cartRepositoryMock.Received(1).GetByIdAsync(cartId);
        await cartRepositoryMock.Received(1).DeleteAsync(existingCart);
        await unitOfWorkMock.Received(1).BeginTransactionAsync();
        await unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        int cartId = 1;

        var cartRepositoryMock = Substitute.For<ICartRepository>();
        var unitOfWorkMock = Substitute.For<IUnitOfWork>();

        cartRepositoryMock.GetByIdAsync(cartId).Returns((Cart?)null);

        var cartService = new CartService(
            cartRepositoryMock,
            unitOfWorkMock,
            Substitute.For<IMapper>(),
            Substitute.For<FluentValidation.IValidator<RequestCartDto>>(),
            Substitute.For<ICartProductRepository>()
        );

        // Act
        var result = await cartService.DeleteAsync(cartId);

        // Assert
        result.Should().NotBeNull("o resultado da operação não deve ser nulo");
        result.IsSuccess.Should().BeFalse("a operação deve falhar quando o carrinho não existe");
        result.StatusCode.Should().Be(404, "o status HTTP deve ser 404 para um carrinho inexistente");
        result.Message.Should().Be($"Carrinho com ID {cartId} não encontrado.", "a mensagem deve indicar que o carrinho não foi encontrado");
        result.Data.Should().BeNull("os dados retornados devem ser nulos quando o carrinho não é encontrado");

        // Verify
        await cartRepositoryMock.Received(1).GetByIdAsync(cartId);
        await cartRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Cart>());
        await unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await unitOfWorkMock.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCart_WhenCartExists()
    {
        // Arrange
        int cartId = 1;

        var existingCart = new Cart
        {
            Id = cartId,
            UserId = 123,
            Date = DateTime.Now.AddDays(-1)
        };

        var existingCartProducts = new List<CartProduct>
        {
            new CartProduct { CartId = cartId, ProductId = 101, Quantity = 2 },
            new CartProduct { CartId = cartId, ProductId = 102, Quantity = 3 }
        };

        var requestDto = new RequestCartDto
        {
            UserId = 456,
            Date = DateTime.Now,
            Products = new List<RequestCartProductDto>
        {
            new RequestCartProductDto { ProductId = 101, Quantity = 5 },
            new RequestCartProductDto { ProductId = 103, Quantity = 2 }
        }
        };

        var updatedCart = new Cart
        {
            Id = cartId,
            UserId = requestDto.UserId,
            Date = requestDto.Date
        };

        var updatedCartDto = new CartDto
        {
            Id = cartId,
            UserId = requestDto.UserId,
            Date = requestDto.Date,
            Products = requestDto.Products.Select(p => new CartProductDto
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity
            }).ToList()
        };

        var validationResultMock = new FluentValidation.Results.ValidationResult();
        _validatorMock.ValidateAsync(Arg.Any<RequestCartDto>()).Returns(validationResultMock);

        _cartRepositoryMock.GetByIdAsync(cartId)
            .Returns(existingCart, updatedCart); 

        _cartProductRepositoryMock.GetByCartIdAsync(cartId).Returns(existingCartProducts);
        _mapperMock.Map<CartDto>(Arg.Any<Cart>()).Returns(updatedCartDto);

        // Act
        var result = await _cartService.UpdateAsync(cartId, requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEquivalentTo(updatedCartDto);

        // Verify
        await _cartRepositoryMock.Received(2).GetByIdAsync(cartId); 

        await _cartProductRepositoryMock.Received(1).GetByCartIdAsync(cartId);

        await _cartProductRepositoryMock.Received(1).UpdateAsync(Arg.Is<CartProduct>(
            cp => cp.ProductId == 101 && cp.Quantity == 5));

        await _cartProductRepositoryMock.Received(1).DeleteAsync(Arg.Is<CartProduct>(
            cp => cp.ProductId == 102));

        await _cartProductRepositoryMock.Received(1).AddAsync(Arg.Is<CartProduct>(
            cp => cp.ProductId == 103 && cp.Quantity == 2));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        // Arrange
        int cartId = 1;

        var requestDto = new RequestCartDto
        {
            UserId = 456,
            Date = DateTime.Now,
            Products = new List<RequestCartProductDto>
        {
            new RequestCartProductDto { ProductId = 101, Quantity = 5 }
        }
        };

        _cartRepositoryMock.GetByIdAsync(cartId).Returns((Cart?)null);

        // Act
        var result = await _cartService.UpdateAsync(cartId, requestDto);

        // Assert
        result.Should().NotBeNull("o método sempre deve retornar uma resposta, mesmo em casos de erro");
        result.IsSuccess.Should().BeFalse("a operação deve falhar quando o carrinho não existe");
        result.StatusCode.Should().Be(404, "o status HTTP deve indicar que o recurso não foi encontrado");
        result.Message.Should().Be($"Carrinho com ID {cartId} não encontrado.", "a mensagem deve indicar o motivo da falha");
        result.Data.Should().BeNull("os dados devem ser nulos quando o carrinho não é encontrado");

        // Verify
        await _cartRepositoryMock.Received(1).GetByIdAsync(cartId);
        await _cartProductRepositoryMock.DidNotReceive().GetByCartIdAsync(cartId);
        await _cartProductRepositoryMock.DidNotReceive().AddAsync(Arg.Any<CartProduct>());
        await _cartProductRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<CartProduct>());
        await _cartProductRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<CartProduct>());
    }
}
