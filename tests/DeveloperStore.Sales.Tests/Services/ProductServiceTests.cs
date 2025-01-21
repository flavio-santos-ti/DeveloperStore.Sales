using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using DeveloperStore.Sales.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class ProductServiceTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IMapper _mapperMock;
    private readonly IProductService _productService;


    public ProductServiceTests()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _mapperMock = Substitute.For<IMapper>();
        _productService = new ProductService(_productRepositoryMock, _mapperMock, Substitute.For<IValidator<RequestProductDto>>(), _unitOfWorkMock);
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

        _productRepositoryMock.GetByIdAsync(productId).Returns(product);
        _mapperMock.Map<ProductDto>(product).Returns(productDto);

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
        _productRepositoryMock.GetByIdAsync(productId).Returns((Product?)null);

        // Act
        var response = await _productService.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(404, response.StatusCode);
        Assert.Equal($"Produto com o ID {productId} não encontrado.", response.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResponse_WhenProductsExist()
    {
        // Arrange
        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Title = "Product A", Price = 10.99m },
            new Product { Id = 2, Title = "Product B", Price = 20.99m },
        };

        var mockQueryable = new TestAsyncEnumerable<Product>(mockProducts.AsQueryable());
        _productRepositoryMock.GetAllQueryable().Returns(mockQueryable);

        _mapperMock
            .Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>())
            .Returns(mockProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price
            }).ToList());

        // Act
        var result = await _productService.GetAllAsync(1, 10, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Data.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNotFound_WhenNoProductsExist()
    {
        // Arrange
        _productRepositoryMock.GetAllQueryable()
            .Returns(new TestAsyncEnumerable<Product>(Enumerable.Empty<Product>().AsQueryable()));

        // Act
        var result = await _productService.GetAllAsync(1, 10, null);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess); 
        Assert.Equal(404, result.StatusCode); 
        Assert.Equal("Nenhum produto encontrado.", result.Message); 
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnPagedResponse_WhenProductsExist()
    {
        // Arrange
        var category = "Electronics";
        var page = 1;
        var size = 10;

        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Title = "Phone", Category = category, Price = 699.99m },
            new Product { Id = 2, Title = "Laptop", Category = category, Price = 1299.99m }
        };

        var mappedProducts = mockProducts.Select(p => new ProductDto
        {
            Id = p.Id,
            Title = p.Title,
            Category = p.Category,
            Price = p.Price
        }).ToList();

        var mockQueryable = new TestAsyncEnumerable<Product>(mockProducts.AsQueryable());

        _productRepositoryMock.GetAllQueryable().Returns(mockQueryable);

        _mapperMock
            .Map<List<ProductDto>>(Arg.Is<IEnumerable<Product>>(x => x.SequenceEqual(mockProducts)))
            .Returns(mappedProducts);

        // Act
        var result = await _productService.GetByCategoryAsync(category, page, size, null);

        // Assert
        result.Should().NotBeNull("O resultado não deve ser nulo"); // Resultado não deve ser nulo
        result.IsSuccess.Should().BeTrue("A operação deve ser bem-sucedida"); // Deve ser sucesso
        result.StatusCode.Should().Be(200, "O status HTTP deve ser 200"); // Status deve ser 200
        result.Data!.Data.Should().NotBeNull("Os dados internos não devem ser nulos"); // Dados internos não nulos
        result.Data!.Data.Count().Should().Be(2, "Devem ser retornados 2 produtos"); // Devem ser 2 produtos

        _mapperMock.Received(1).Map<List<ProductDto>>(
            Arg.Is<IEnumerable<Product>>(x => x.SequenceEqual(mockProducts))
        );

        _productRepositoryMock.Received(1).GetAllQueryable();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnNotFound_WhenNoProductsExist()
    {
        // Arrange
        var category = "NonExistentCategory";
        var page = 1;
        var size = 10;

        var emptyQueryable = new TestAsyncEnumerable<Product>(Enumerable.Empty<Product>().AsQueryable());
        _productRepositoryMock.GetAllQueryable().Returns(emptyQueryable);

        // Act
        var result = await _productService.GetByCategoryAsync(category, page, size, null);

        // Assert
        result.Should().NotBeNull(); 
        result.IsSuccess.Should().BeFalse(); 
        result.StatusCode.Should().Be(404); 
        result.Message.Should().Be($"Nenhum produto encontrado na categoria '{category}'.");

        _productRepositoryMock.Received(1).GetAllQueryable();
        _mapperMock.DidNotReceive().Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>());
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnBadRequest_WhenCategoryIsInvalid()
    {
        // Arrange
        var invalidCategory = "";
        var page = 1;
        var size = 10;

        // Act
        var result = await _productService.GetByCategoryAsync(invalidCategory, page, size, null);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("A categoria não pode ser nula ou vazia.");

        // Verify no interactions with repository or mapper
        _productRepositoryMock.DidNotReceive().GetAllQueryable();
        _mapperMock.DidNotReceive().Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>());
    }

}
