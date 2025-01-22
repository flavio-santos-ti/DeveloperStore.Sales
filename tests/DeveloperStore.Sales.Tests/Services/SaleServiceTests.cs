using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Sale;
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
}
