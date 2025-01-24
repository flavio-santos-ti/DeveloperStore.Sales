using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.UnitOfWork;
using DeveloperStore.Sales.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class ProductServiceTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IMapper _mapperMock;
    private readonly IProductService _productService;
    private readonly IValidator<RequestProductDto> _validatorMock;

    public ProductServiceTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _mapperMock = Substitute.For<IMapper>();
        _validatorMock = Substitute.For<IValidator<RequestProductDto>>();
        _productService = new ProductService(_mapperMock, _validatorMock, _unitOfWorkMock);
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

        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns(product);
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
        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns((Product?)null);

        // Act
        var response = await _productService.GetByIdAsync(productId);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(404, response.StatusCode);
        Assert.Equal($"Produto com o ID {productId} não encontrado.", response.Message);
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

        _unitOfWorkMock.ProductRepository.GetAllQueryable().Returns(mockQueryable);

        _mapperMock
            .Map<List<ProductDto>>(Arg.Is<IEnumerable<Product>>(x => x.SequenceEqual(mockProducts)))
            .Returns(mappedProducts);

        // Act
        var result = await _productService.GetByCategoryAsync(category, page, size, null);

        // Assert
        result.Should().NotBeNull("O resultado não deve ser nulo"); 
        result.IsSuccess.Should().BeTrue("A operação deve ser bem-sucedida"); 
        result.StatusCode.Should().Be(200, "O status HTTP deve ser 200"); 
        result.Data!.Data.Should().NotBeNull("Os dados internos não devem ser nulos"); 
        result.Data!.Data.Count().Should().Be(2, "Devem ser retornados 2 produtos"); 

        _mapperMock.Received(1).Map<List<ProductDto>>(
            Arg.Is<IEnumerable<Product>>(x => x.SequenceEqual(mockProducts))
        );

        _unitOfWorkMock.ProductRepository.Received(1).GetAllQueryable();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnNotFound_WhenNoProductsExist()
    {
        // Arrange
        var category = "NonExistentCategory";
        var page = 1;
        var size = 10;

        var emptyQueryable = new TestAsyncEnumerable<Product>(Enumerable.Empty<Product>().AsQueryable());
        _unitOfWorkMock.ProductRepository.GetAllQueryable().Returns(emptyQueryable);

        // Act
        var result = await _productService.GetByCategoryAsync(category, page, size, null);

        // Assert
        result.Should().NotBeNull(); 
        result.IsSuccess.Should().BeFalse(); 
        result.StatusCode.Should().Be(404); 
        result.Message.Should().Be($"Nenhum produto encontrado na categoria '{category}'.");

        _unitOfWorkMock.ProductRepository.Received(1).GetAllQueryable();
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

        // Verify
        _unitOfWorkMock.ProductRepository.DidNotReceive().GetAllQueryable();
        _mapperMock.DidNotReceive().Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var existingProduct = new Product
        {
            Id = productId,
            Title = "Test Product",
            Price = 100.0m,
            Description = "A sample product for testing",
            Category = "TestCategory"
        };

        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns(existingProduct);

        // Act
        var response = await _productService.DeleteAsync(productId);

        // Assert
        response.Should().NotBeNull("O resultado não deve ser nulo");
        response.IsSuccess.Should().BeTrue("Deve ser sucesso");
        response.StatusCode.Should().Be(200, "O status HTTP deve ser 200");
        response.Message.Should().Be($"Produto com o ID {productId} excluído com sucesso.");

        // Verify 
        await _unitOfWorkMock.ProductRepository.Received(1).GetByIdAsync(productId);
        await _unitOfWorkMock.ProductRepository.Received(1).DeleteAsync(existingProduct);
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = 999;
        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns((Product?)null);

        // Act
        var response = await _productService.DeleteAsync(productId);

        // Assert
        response.Should().NotBeNull("O resultado não deve ser nulo");
        response.IsSuccess.Should().BeFalse("Deve ser falha");
        response.StatusCode.Should().Be(404, "O status HTTP deve ser 404 para recursos não encontrados");
        response.Message.Should().Be($"Produto com o ID {productId} não encontrado.");

        // Verify 
        await _unitOfWorkMock.ProductRepository.Received(1).GetByIdAsync(productId);
        await _unitOfWorkMock.ProductRepository.DidNotReceive().DeleteAsync(Arg.Any<Product>());
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenProductIsUpdated()
    {
        // Arrange
        var productId = 1;
        var requestDto = new RequestProductDto
        {
            Title = "Updated Product",
            Price = 150.0m,
            Description = "Updated description",
            Category = "Updated Category",
            Image = "http://example.com/updated-image.jpg"
        };

        var existingProduct = new Product
        {
            Id = productId,
            Title = "Original Product",
            Price = 100.0m,
            Description = "Original description",
            Category = "Original Category",
            Image = "http://example.com/original-image.jpg"
        };

        var validationResult = new FluentValidation.Results.ValidationResult();

        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns(existingProduct);
        _validatorMock.ValidateAsync(requestDto).Returns(validationResult);

        var updatedProductDto = new ProductDto
        {
            Id = productId,
            Title = requestDto.Title,
            Price = requestDto.Price,
            Description = requestDto.Description,
            Category = requestDto.Category,
            Image = requestDto.Image
        };

        _mapperMock.Map(requestDto, existingProduct).Returns(existingProduct);
        _mapperMock.Map<ProductDto>(existingProduct).Returns(updatedProductDto);

        // Act
        var response = await _productService.UpdateAsync(productId, requestDto);

        // Assert
        response.Should().NotBeNull("O resultado não deve ser nulo");
        response.IsSuccess.Should().BeTrue("A operação deve ser bem-sucedida");
        response.StatusCode.Should().Be(200, "O status HTTP deve ser 200");
        response.Data.Should().BeEquivalentTo(updatedProductDto, "Os dados retornados devem corresponder ao produto atualizado");

        // Verify
        await _unitOfWorkMock.ProductRepository.Received(1).GetByIdAsync(productId);
        await _unitOfWorkMock.ProductRepository.Received(1).UpdateAsync(existingProduct);
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.Received(1).CommitAsync();
        await _validatorMock.Received(1).ValidateAsync(requestDto);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = 999;
        var requestProductDto = new RequestProductDto
        {
            Title = "NonExistent Product",
            Price = 100.00m,
            Description = "Non-existent description",
            Category = "NonExistentCategory",
            Image = "nonexistent.jpg"
        };

        var validationResult = new FluentValidation.Results.ValidationResult(); // Sem erros de validação
        _validatorMock.ValidateAsync(requestProductDto, Arg.Any<CancellationToken>())
                      .Returns(validationResult);

        _unitOfWorkMock.ProductRepository.GetByIdAsync(productId).Returns((Product?)null);

        // Act
        var response = await _productService.UpdateAsync(productId, requestProductDto);

        // Assert
        response.Should().NotBeNull("O resultado não deve ser nulo");
        response.IsSuccess.Should().BeFalse("Deve ser falha");
        response.StatusCode.Should().Be(404, "O status HTTP deve ser 404 para recursos não encontrados");
        response.Message.Should().Be($"Produto com o ID {productId} não encontrado.");

        // Verify
        await _unitOfWorkMock.ProductRepository.Received(1).GetByIdAsync(productId);
        _mapperMock.DidNotReceive().Map(requestProductDto, Arg.Any<Product>());
        await _unitOfWorkMock.ProductRepository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.DidNotReceive().CommitAsync();
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarSucesso_QuandoProdutoForCriado()
    {
        // Arrange
        var requestDto = new RequestProductDto
        {
            Title = "Produto Novo",
            Price = 99.99m,
            Description = "Este é um produto de teste",
            Category = "CategoriaTeste",
            Image = "http://example.com/imagem.jpg",
            Rating = new ProductRating { Rate = 4.5m, Count = 100 }
        };

        var validationResult = new FluentValidation.Results.ValidationResult(); // Sem erros de validação
        _validatorMock.ValidateAsync(requestDto, Arg.Any<CancellationToken>()).Returns(validationResult);

        _unitOfWorkMock.ProductRepository.ExistsByTitleAsync(requestDto.Title).Returns(false);

        var createdProduct = new Product
        {
            Id = 1,
            Title = requestDto.Title,
            Price = requestDto.Price,
            Description = requestDto.Description,
            Category = requestDto.Category,
            Image = requestDto.Image,
            Rating = new ProductRating { Rate = requestDto.Rating.Rate, Count = requestDto.Rating.Count }
        };

        var createdProductDto = new ProductDto
        {
            Id = createdProduct.Id,
            Title = createdProduct.Title,
            Price = createdProduct.Price,
            Description = createdProduct.Description,
            Category = createdProduct.Category,
            Image = createdProduct.Image,
            Rating = new ProductRating { Rate = createdProduct.Rating.Rate, Count = createdProduct.Rating.Count }
        };

        _mapperMock.Map<Product>(requestDto).Returns(createdProduct);
        _mapperMock.Map<ProductDto>(createdProduct).Returns(createdProductDto);

        // Act
        var response = await _productService.CreateAsync(requestDto);

        // Assert
        response.Should().NotBeNull("O resultado não deve ser nulo");
        response.IsSuccess.Should().BeTrue("A operação deve ser bem-sucedida");
        response.StatusCode.Should().Be(201, "O status HTTP deve ser 201 para criação");
        response.Data.Should().BeEquivalentTo(createdProductDto, "Os dados retornados devem corresponder ao produto criado");

        // Verify
        await _validatorMock.Received(1).ValidateAsync(requestDto, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.ProductRepository.Received(1).ExistsByTitleAsync(requestDto.Title);
        await _unitOfWorkMock.ProductRepository.Received(1).AddAsync(createdProduct);
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResponse_WhenProductsExist()
    {
        // Arrange
        var mockProducts = new List<Product>
        {
            new Product { Id = 1, Title = "Product A", Price = 10.99m, Category = "Category A" },
            new Product { Id = 2, Title = "Product B", Price = 20.99m, Category = "Category B" },
            new Product { Id = 3, Title = "Product C", Price = 15.50m, Category = "Category A" }
        };

        var filters = new Dictionary<string, string>
        {
            { "category", "Category A" }, 
            { "title", "Product*" }      
        };

        var filteredProducts = mockProducts
            .Where(p => p.Category == "Category A" && p.Title.StartsWith("Product"))
            .OrderByDescending(p => p.Price) 
            .Skip(0) 
            .Take(2)
            .ToList();

        var mockQueryable = new TestAsyncEnumerable<Product>(filteredProducts.AsQueryable());
        _unitOfWorkMock.ProductRepository.GetAllQueryable().Returns(mockQueryable);

        _mapperMock
            .Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>())
            .Returns(filteredProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                Category = p.Category
            }).ToList());

        // Act
        var result = await _productService.GetAllAsync(1, 2, "price desc", filters);

        // Assert
        result.Should().NotBeNull("O resultado não deve ser nulo");
        result.IsSuccess.Should().BeTrue("A operação deve ser bem-sucedida");
        result.StatusCode.Should().Be(200, "O status HTTP deve ser 200");
        result.Data.Should().NotBeNull("Os dados não devem ser nulos");
        result.Data!.Data.Should().HaveCount(2, "Devem ser retornados dois produtos devido à paginação");
        result.Data!.Data.First().Price.Should().Be(15.50m, "O produto com o maior preço deve vir primeiro devido à ordenação decrescente");

        // Verify 
        _unitOfWorkMock.ProductRepository.Received(1).GetAllQueryable();
        _mapperMock.Received(1).Map<IEnumerable<ProductDto>>(
            Arg.Is<IEnumerable<Product>>(x => x.Count() == 2));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNotFound_WhenNoProductsMatchFilters()
    {
        // Arrange
        var filters = new Dictionary<string, string>
        {
            { "category", "NonExistentCategory" }, // Invalid category
            { "title", "NonExistentProduct" }      // Invalid title
        };

        _unitOfWorkMock.ProductRepository.GetAllQueryable()
            .Returns(new TestAsyncEnumerable<Product>(Enumerable.Empty<Product>().AsQueryable()));

        // Act
        var result = await _productService.GetAllAsync(1, 10, null, filters);

        // Assert
        result.Should().NotBeNull("O resultado não deve ser nulo");
        result.IsSuccess.Should().BeFalse("A operação deve falhar");
        result.StatusCode.Should().Be(404, "O status HTTP deve ser 404 para nenhuma correspondência");
        result.Message.Should().Be("Nenhum produto encontrado.");

        // Verify
        _unitOfWorkMock.ProductRepository.Received(1).GetAllQueryable();
        _mapperMock.DidNotReceive().Map<IEnumerable<ProductDto>>(Arg.Any<IEnumerable<Product>>());
    }

}
