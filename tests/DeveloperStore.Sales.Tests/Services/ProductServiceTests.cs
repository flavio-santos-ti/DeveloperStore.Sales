using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentValidation;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;


        public ProductServiceTests()
        {
            _productRepository = Substitute.For<IProductRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _productService = new ProductService(_productRepository, _mapper, Substitute.For<IValidator<RequestProductDto>>(), _unitOfWork);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Title = "Test Product",
                Price = 100.0m,
                Description = "A sample product for testing",
                Category = "TestCategory",
                Image = "test.jpg",
                Rating = new ProductRating { Rate = 4.5m, Count = 10 }
            };
            var productDto = new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                Image = product.Image,
                Rating = new ProductRating { Rate = product.Rating.Rate, Count = product.Rating.Count }
            };

            _productRepository.GetByIdAsync(productId).Returns(product);
            _mapper.Map<ProductDto>(product).Returns(productDto);

            // Act
            var response = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.Equal(productDto.Id, response.Data?.Id);
            Assert.Equal(productDto.Title, response.Data?.Title);
            Assert.Equal(productDto.Price, response.Data?.Price);
            Assert.Equal(productDto.Description, response.Data?.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999; 
            _productRepository.GetByIdAsync(productId).Returns((Product?)null);

            // Act
            var response = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal(404, response.StatusCode);
            Assert.Equal($"Produto com o ID {productId} não encontrado.", response.Message);
        }
    }
}
