using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using DeveloperStore.Sales.Tests.Helpers;
using FluentAssertions;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services
{
    public class CartServiceTests
    {
        private readonly ICartRepository _cartRepositoryMock;
        private readonly ICartProductRepository _cartProductRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepositoryMock = Substitute.For<ICartRepository>();
            _cartProductRepositoryMock = Substitute.For<ICartProductRepository>();
            _mapperMock = Substitute.For<IMapper>();

            _cartService = new CartService(
                _cartRepositoryMock,
                Substitute.For<IUnitOfWork>(),
                _mapperMock,
                Substitute.For<FluentValidation.IValidator<RequestCartDto>>(),
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
    }
}
