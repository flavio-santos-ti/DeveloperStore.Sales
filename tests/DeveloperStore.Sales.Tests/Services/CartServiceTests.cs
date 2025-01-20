using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services
{
    public class CartServiceTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartProductRepository _cartProductRepository;
        private readonly IMapper _mapper;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            // Mocks
            _cartRepository = Substitute.For<ICartRepository>();
            _cartProductRepository = Substitute.For<ICartProductRepository>();
            _mapper = Substitute.For<IMapper>();

            // Serviço a ser testado
            _cartService = new CartService(
                _cartRepository,
                Substitute.For<IUnitOfWork>(),
                _mapper,
                Substitute.For<FluentValidation.IValidator<RequestCartDto>>(),
                _cartProductRepository
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

            // Configurando os mocks
            _cartRepository.GetByIdAsync(cartId).Returns(cart);
            _cartProductRepository.GetByCartIdAsync(cartId).Returns(cartProducts);
            _mapper.Map<CartDto>(cart).Returns(expectedCartDto);

            // Act
            var response = await _cartService.GetByIdAsync(cartId);

            // Assert
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.StatusCode.Should().Be(200);
            response.Data.Should().NotBeNull();
            response.Data.Should().BeEquivalentTo(expectedCartDto);

            // Verificando chamadas nos mocks
            await _cartRepository.Received(1).GetByIdAsync(cartId);
            await _cartProductRepository.Received(1).GetByCartIdAsync(cartId);
            _mapper.Received(1).Map<CartDto>(cart);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
        {
            // Arrange
            int cartId = 1;

            _cartRepository.GetByIdAsync(cartId).Returns((Cart?)null);

            // Act
            var response = await _cartService.GetByIdAsync(cartId);

            // Assert
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeFalse();
            response.StatusCode.Should().Be(404);
            response.Data.Should().BeNull();
            response.Message.Should().Be($"Carrinho com ID {cartId} não encontrado.");

            // Verificando chamadas nos mocks
            await _cartRepository.Received(1).GetByIdAsync(cartId);
            await _cartProductRepository.DidNotReceive().GetByCartIdAsync(cartId);
            _mapper.DidNotReceive().Map<CartDto>(Arg.Any<Cart>());
        }
    }
}
