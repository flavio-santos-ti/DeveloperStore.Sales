using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Sale;
using DeveloperStore.Sales.Domain.Events;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace DeveloperStore.Sales.Tests.Services;

public class SaleServiceTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IMapper _mapperMock;
    private readonly IMediator _mediatorMock;
    private readonly SaleService _saleService;

    public SaleServiceTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _mapperMock = Substitute.For<IMapper>();
        _mediatorMock = Substitute.For<IMediator>();
        _saleService = new SaleService(_unitOfWorkMock, _mapperMock, _mediatorMock);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await _saleService.CreateAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("A venda deve conter pelo menos um item.");

        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenDtoHasNoItems()
    {
        // Arrange
        var requestDto = new RequestSaleDto
        {
            CustomerId = 1,
            Branch = "Test Branch",
            Items = new List<RequestSaleItemDto>()
        };

        // Act
        var result = await _saleService.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("A venda deve conter pelo menos um item.");

        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequest_WhenQuantityExceedsLimit()
    {
        // Arrange
        var requestDto = new RequestSaleDto
        {
            CustomerId = 1,
            Branch = "Test Branch",
            Items = new List<RequestSaleItemDto>
        {
            new RequestSaleItemDto
            {
                ProductId = 1,
                Quantity = 25, // Exceeds limit
                UnitPrice = 10
            }
        }
        };

        // Act
        var result = await _saleService.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Não é permitido vender mais de 20 itens idênticos.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSaleAndPublishEvent_WhenDtoIsValid()
    {
        // Arrange
        var requestDto = new RequestSaleDto
        {
            CustomerId = 1,
            Branch = "Test Branch",
            Items = new List<RequestSaleItemDto>
        {
            new RequestSaleItemDto { ProductId = 1, Quantity = 5, UnitPrice = 20 },
            new RequestSaleItemDto { ProductId = 2, Quantity = 10, UnitPrice = 15 }
        }
        };

        var sale = new Sale
        {
            Id = 1,
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow,
            CustomerId = requestDto.CustomerId,
            Branch = requestDto.Branch,
            TotalAmount = 275,
            Items = new List<SaleItem>
        {
            new SaleItem { ProductId = 1, Quantity = 5, UnitPrice = 20, Discount = 0, TotalAmount = 100 },
            new SaleItem { ProductId = 2, Quantity = 10, UnitPrice = 15, Discount = 30, TotalAmount = 150 }
        }
        };

        var saleRepositoryMock = Substitute.For<ISaleRepository>();
        _unitOfWorkMock.SaleRepository.Returns(saleRepositoryMock);

        _mapperMock.Map<SaleDto>(Arg.Any<Sale>()).Returns(new SaleDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            CustomerId = sale.CustomerId,
            Branch = sale.Branch,
            TotalAmount = sale.TotalAmount,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount
            }).ToList()
        });

        // Act
        var result = await _saleService.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.TotalAmount.Should().Be(275);

        // Verify
        await saleRepositoryMock.Received(1).AddAsync(Arg.Any<Sale>());
        await _unitOfWorkMock.Received(1).CommitAsync();
    }

    [Fact]
    public async Task CancelSaleAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        // Arrange
        var saleId = 1;
        _unitOfWorkMock.SaleRepository.GetByIdAsync(saleId).Returns((Sale)null);

        // Act
        var result = await _saleService.CancelSaleAsync(saleId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Venda não encontrada.");

        // Verify
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        await _mediatorMock.DidNotReceive().Publish(Arg.Any<SaleCancelledEvent>());
    }

    [Fact]
    public async Task CancelSaleAsync_ShouldReturnBadRequest_WhenSaleIsAlreadyCancelled()
    {
        // Arrange
        var sale = new Sale
        {
            Id = 1,
            IsCancelled = true
        };
        _unitOfWorkMock.SaleRepository.GetByIdAsync(sale.Id).Returns(sale);

        // Act
        var result = await _saleService.CancelSaleAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Venda já foi cancelada.");

        // Verify
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        await _mediatorMock.DidNotReceive().Publish(Arg.Any<SaleCancelledEvent>());
    }

    [Fact]
    public async Task CancelSaleAsync_ShouldCancelSaleAndPublishEvent_WhenSaleIsValid()
    {
        // Arrange
        var sale = new Sale
        {
            Id = 1,
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow,
            CustomerId = 1,
            TotalAmount = 100,
            IsCancelled = false
        };
        _unitOfWorkMock.SaleRepository.GetByIdAsync(sale.Id).Returns(sale);

        // Act
        var result = await _saleService.CancelSaleAsync(sale.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be($"Venda {sale.SaleNumber} cancelada com sucesso.");

        // Verify
        await _unitOfWorkMock.Received(1).BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.IsCancelled));
        await _unitOfWorkMock.Received(1).CommitAsync();

        await _mediatorMock.Received(1).Publish(Arg.Is<SaleCancelledEvent>(e =>
            e.SaleId == sale.Id &&
            e.SaleNumber == sale.SaleNumber &&
            e.CustomerId == sale.CustomerId &&
            e.TotalAmount == sale.TotalAmount
        ));
    }

    [Fact]
    public async Task UpdateSaleAsync_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        // Arrange
        var saleId = 1;
        RequestSaleDto? dto = null;

        // Act
        var result = await _saleService.UpdateSaleAsync(saleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("A venda deve conter pelo menos um item.");

        // Verify
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        await _mediatorMock.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>());
    }

    [Fact]
    public async Task UpdateSaleAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        // Arrange
        var saleId = 1;
        var dto = new RequestSaleDto
        {
            CustomerId = 1,
            Branch = "Test Branch",
            Items = new List<RequestSaleItemDto>
        {
            new RequestSaleItemDto { ProductId = 1, Quantity = 5, UnitPrice = 10 }
        }
        };

        _unitOfWorkMock.SaleRepository.GetByIdAsync(saleId).Returns((Sale)null);

        // Act
        var result = await _saleService.UpdateSaleAsync(saleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Be("Venda não encontrada.");

        // Verify
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        await _mediatorMock.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>());
    }

    [Fact]
    public async Task UpdateSaleAsync_ShouldReturnBadRequest_WhenDtoHasNoItems()
    {
        // Arrange
        var saleId = 1;
        var dto = new RequestSaleDto
        {
            CustomerId = 1,
            Branch = "Test Branch",
            Items = new List<RequestSaleItemDto>()
        };

        _unitOfWorkMock.SaleRepository.GetByIdAsync(saleId).Returns(new Sale { Id = saleId });

        // Act
        var result = await _saleService.UpdateSaleAsync(saleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("A venda deve conter pelo menos um item.");

        // Verify
        await _unitOfWorkMock.DidNotReceive().BeginTransactionAsync();
        await _unitOfWorkMock.SaleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
        await _mediatorMock.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>());
    }

    [Fact]
    public async Task UpdateSaleAsync_ShouldUpdateSaleAndPublishEvent_WhenDtoIsValid()
    {
        // Arrange
        var saleId = 1;
        var dto = new RequestSaleDto
        {
            CustomerId = 2,
            Branch = "Updated Branch",
            Items = new List<RequestSaleItemDto>
            {
                new RequestSaleItemDto { ProductId = 1, Quantity = 5, UnitPrice = 20 },
                new RequestSaleItemDto { ProductId = 2, Quantity = 10, UnitPrice = 15 }
            }
        };

        var existingSale = new Sale
        {
            Id = saleId,
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = 1,
            Branch = "Original Branch",
            TotalAmount = 100,
            Items = new List<SaleItem>
            {
                new SaleItem { ProductId = 1, Quantity = 2, UnitPrice = 10, Discount = 0, TotalAmount = 20 }
            }
        };

        _unitOfWorkMock.SaleRepository.GetByIdAsync(saleId).Returns(existingSale);

        Sale capturedSale = null;
        _unitOfWorkMock.SaleRepository
            .When(x => x.UpdateAsync(Arg.Any<Sale>()))
            .Do(callInfo => {
                capturedSale = callInfo.Arg<Sale>();
            });

        // Act
        var result = await _saleService.UpdateSaleAsync(saleId, dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be($"Venda {existingSale.SaleNumber} atualizada com sucesso.");

        capturedSale.Should().NotBeNull();
        Console.WriteLine($"Captured Sale: Branch={capturedSale.Branch}, TotalAmount={capturedSale.TotalAmount}, Items={capturedSale.Items.Count}");

        capturedSale.Branch.Should().Be(dto.Branch);
        capturedSale.CustomerId.Should().Be(dto.CustomerId);
        capturedSale.TotalAmount.Should().Be(210);
        capturedSale.Items.Should().HaveCount(2);
        capturedSale.Items.Should().ContainSingle(i => i.ProductId == 1 && i.Quantity == 5 && i.TotalAmount == 90);
        capturedSale.Items.Should().ContainSingle(i => i.ProductId == 2 && i.Quantity == 10 && i.TotalAmount == 120);


        await _mediatorMock.Received(1).Publish(Arg.Is<SaleModifiedEvent>(e =>
            e.SaleId == saleId &&
            e.SaleNumber == existingSale.SaleNumber &&
            e.CustomerId == dto.CustomerId &&
            e.NewTotalAmount == 210
        ));
    }

    [Fact]
    public async Task CancelSaleItemAsync_ShouldRemoveItemAndUpdateSale_WhenItemExists()
    {
        // Arrange
        var saleId = 1;
        var itemId = 2;

        var sale = new Sale
        {
            Id = saleId,
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = 1,
            Branch = "Original Branch",
            TotalAmount = 200,
            Items = new List<SaleItem>
        {
            new SaleItem { Id = 1, ProductId = 1, Quantity = 2, UnitPrice = 50, Discount = 0, TotalAmount = 100 },
            new SaleItem { Id = itemId, ProductId = 2, Quantity = 2, UnitPrice = 50, Discount = 0, TotalAmount = 100 }
        }
        };

        _unitOfWorkMock.SaleRepository.GetByIdAsync(saleId).Returns(sale);

        Sale capturedSale = null;
        _unitOfWorkMock.SaleRepository
            .When(x => x.UpdateAsync(Arg.Any<Sale>()))
            .Do(callInfo => {
                capturedSale = callInfo.Arg<Sale>();
            });

        // Act
        var result = await _saleService.CancelSaleItemAsync(saleId, itemId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be($"Item {itemId} da venda {sale.SaleNumber} foi cancelado com sucesso.");

        capturedSale.Should().NotBeNull();
        capturedSale.Items.Should().HaveCount(1); 
        capturedSale.Items.Should().NotContain(i => i.Id == itemId);
        capturedSale.TotalAmount.Should().Be(100); 

        await _mediatorMock.Received(1).Publish(Arg.Is<ItemCancelledEvent>(e =>
            e.SaleId == saleId &&
            e.SaleNumber == sale.SaleNumber &&
            e.ItemId == itemId &&
            e.ProductId == 2 &&
            e.Quantity == 2 &&
            e.UnitPrice == 50 &&
            e.TotalAmount == 100 &&
            e.CancelledDate <= DateTime.UtcNow
        ));
    }

    [Fact]
    public async Task CheckoutCartAsync_ShouldCreateSaleAndPublishEvent_WhenCartIsValid()
    {
        // Arrange
        var cartId = 1;

        var cart = new Cart
        {
            Id = cartId,
            UserId = 123,
            CartProducts = new List<CartProduct>
        {
            new CartProduct { ProductId = 1, Quantity = 2 },
            new CartProduct { ProductId = 2, Quantity = 3 }
        }
        };

        var products = new List<Product>
    {
        new Product { Id = 1, Price = 50 },
        new Product { Id = 2, Price = 30 }
    };

        var cartTotal = 50 * 2 + 30 * 3; // Total = 190

        _unitOfWorkMock.CartRepository.GetByIdAsync(cartId).Returns(cart);
        _unitOfWorkMock.ProductRepository.GetByIdAsync(Arg.Is<int>(id => id == 1)).Returns(products[0]);
        _unitOfWorkMock.ProductRepository.GetByIdAsync(Arg.Is<int>(id => id == 2)).Returns(products[1]);

        Sale capturedSale = null;
        _unitOfWorkMock.SaleRepository
            .When(x => x.AddAsync(Arg.Any<Sale>()))
            .Do(callInfo => {
                capturedSale = callInfo.Arg<Sale>();
            });

        _mapperMock.Map<SaleDto>(Arg.Any<Sale>()).Returns(new SaleDto
        {
            Id = 1,
            SaleNumber = "12345",
            SaleDate = DateTime.UtcNow,
            CustomerId = cart.UserId,
            Branch = "Default Branch",
            TotalAmount = cartTotal,
            Items = new List<SaleItemDto>
        {
            new SaleItemDto { ProductId = 1, Quantity = 2, UnitPrice = 50, TotalAmount = 100 },
            new SaleItemDto { ProductId = 2, Quantity = 3, UnitPrice = 30, TotalAmount = 90 }
        }
        });

        // Act
        var result = await _saleService.CheckoutCartAsync(cartId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Be("Resource created successfully");
        result.Data.Should().NotBeNull();

        capturedSale.Should().NotBeNull();
        capturedSale.CustomerId.Should().Be(cart.UserId);
        capturedSale.Branch.Should().Be("Default Branch");
        capturedSale.TotalAmount.Should().Be(cartTotal);
        capturedSale.Items.Should().HaveCount(2);

        capturedSale.Items.Should().ContainSingle(i => i.ProductId == 1 && i.Quantity == 2 && i.TotalAmount == 100);
        capturedSale.Items.Should().ContainSingle(i => i.ProductId == 2 && i.Quantity == 3 && i.TotalAmount == 90);

        await _mediatorMock.Received(1).Publish(Arg.Is<SaleCreatedEvent>(e =>
            e.SaleId == capturedSale.Id &&
            e.SaleNumber == capturedSale.SaleNumber &&
            e.CustomerId == capturedSale.CustomerId &&
            e.TotalAmount == capturedSale.TotalAmount
        ));
    }
}
