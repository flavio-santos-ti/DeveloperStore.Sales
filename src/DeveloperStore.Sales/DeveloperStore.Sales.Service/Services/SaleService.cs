using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.Sale;
using DeveloperStore.Sales.Domain.Dtos.Sarle;
using DeveloperStore.Sales.Domain.Events;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.Interfaces;
using MediatR;

namespace DeveloperStore.Sales.Service.Services;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public SaleService(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    private decimal CalculateDiscount(int quantity, decimal unitPrice)
    {
        if (quantity >= 10 && quantity <= 20)
            return unitPrice * 0.2m;
        if (quantity >= 4)
            return unitPrice * 0.1m;
        return 0;
    }

    public async Task<ApiResponseDto<SaleDto>> CreateAsync(RequestSaleDto dto)
    {
        if (dto == null || !dto.Items.Any())
            return ApiResponseDto<SaleDto>.AsBadRequest("A venda deve conter pelo menos um item.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var sale = new Sale
            {
                SaleNumber = Guid.NewGuid().ToString(),
                SaleDate = DateTime.UtcNow,
                CustomerId = dto.CustomerId,
                Branch = dto.Branch,
                TotalAmount = 0
            };

            foreach (var item in dto.Items)
            {
                if (item.Quantity > 20)
                    throw new InvalidOperationException("Não é permitido vender mais de 20 itens idênticos.");

                var discount = CalculateDiscount(item.Quantity, item.UnitPrice);
                var totalAmount = item.Quantity * (item.UnitPrice - discount);

                sale.Items.Add(new SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = discount,
                    TotalAmount = totalAmount
                });

                sale.TotalAmount += totalAmount;
            }

            await _unitOfWork.SaleRepository.AddAsync(sale);
            await _unitOfWork.CommitAsync();

            var saleCreatedEvent = new SaleCreatedEvent(sale.Id, sale.SaleNumber, sale.SaleDate, sale.CustomerId, sale.TotalAmount);
            await _mediator.Publish(saleCreatedEvent);

            var saleDto = _mapper.Map<SaleDto>(sale);
            return ApiResponseDto<SaleDto>.AsCreated(saleDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<SaleDto>.AsInternalServerError($"Erro ao criar venda: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<string>> CancelSaleAsync(int saleId)
    {
        var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
        if (sale == null)
            return ApiResponseDto<string>.AsNotFound("Venda não encontrada.");

        if (sale.IsCancelled)
            return ApiResponseDto<string>.AsBadRequest("Venda já foi cancelada.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            sale.IsCancelled = true;
            await _unitOfWork.SaleRepository.UpdateAsync(sale);
            await _unitOfWork.CommitAsync();

            var saleCancelledEvent = new SaleCancelledEvent(
                sale.Id,              
                sale.SaleNumber,      
                sale.SaleDate,        
                DateTime.UtcNow,      
                sale.CustomerId,      
                sale.TotalAmount      
            );

            await _mediator.Publish(saleCancelledEvent);

            return ApiResponseDto<string>.AsSuccess($"Venda {sale.SaleNumber} cancelada com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<string>.AsInternalServerError($"Erro ao cancelar venda: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<string>> UpdateSaleAsync(int saleId, RequestSaleDto dto)
    {
        if (dto == null || !dto.Items.Any())
            return ApiResponseDto<string>.AsBadRequest("A venda deve conter pelo menos um item.");

        var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
        if (sale == null)
            return ApiResponseDto<string>.AsNotFound("Venda não encontrada.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            sale.Branch = dto.Branch;
            sale.CustomerId = dto.CustomerId;
            sale.SaleDate = DateTime.UtcNow;
            sale.Items.Clear(); 
            sale.TotalAmount = 0;

            foreach (var item in dto.Items)
            {
                if (item.Quantity > 20)
                    throw new InvalidOperationException("Não é permitido vender mais de 20 itens idênticos.");

                var discount = CalculateDiscount(item.Quantity, item.UnitPrice);
                var totalAmount = item.Quantity * (item.UnitPrice - discount);

                sale.Items.Add(new SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = discount,
                    TotalAmount = totalAmount
                });

                sale.TotalAmount += totalAmount;
            }

            await _unitOfWork.SaleRepository.UpdateAsync(sale);
            await _unitOfWork.CommitAsync();

            var saleModifiedEvent = new SaleModifiedEvent(
                sale.Id,
                sale.SaleNumber,
                sale.SaleDate,
                sale.CustomerId,
                sale.TotalAmount
            );
            await _mediator.Publish(saleModifiedEvent);

            return ApiResponseDto<string>.AsSuccess($"Venda {sale.SaleNumber} atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<string>.AsInternalServerError($"Erro ao atualizar venda: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<string>> CancelSaleItemAsync(int saleId, int itemId)
    {
        var sale = await _unitOfWork.SaleRepository.GetByIdAsync(saleId);
        if (sale == null)
            return ApiResponseDto<string>.AsNotFound("Venda não encontrada.");

        var item = sale.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return ApiResponseDto<string>.AsNotFound("Item da venda não encontrado.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            sale.Items.Remove(item);

            sale.TotalAmount = sale.Items.Sum(i => i.TotalAmount);

            await _unitOfWork.SaleRepository.UpdateAsync(sale);
            await _unitOfWork.CommitAsync();

            var itemCancelledEvent = new ItemCancelledEvent(
                sale.Id,
                sale.SaleNumber,
                item.Id,
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.TotalAmount,
                DateTime.UtcNow
            );
            await _mediator.Publish(itemCancelledEvent);

            return ApiResponseDto<string>.AsSuccess($"Item {itemId} da venda {sale.SaleNumber} foi cancelado com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<string>.AsInternalServerError($"Erro ao cancelar item da venda: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<SaleDto>> CheckoutCartAsync(int cartId)
    {
        var cart = await _unitOfWork.CartRepository.GetByIdAsync(cartId);
        if (cart == null || !cart.CartProducts.Any())
            return ApiResponseDto<SaleDto>.AsNotFound("Carrinho não encontrado ou vazio.");
        
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var sale = new Sale
            {
                SaleNumber = Guid.NewGuid().ToString(),
                SaleDate = DateTime.UtcNow,
                CustomerId = cart.UserId,
                Branch = "Default Branch", 
                TotalAmount = 0
            };

            foreach (var cartProduct in cart.CartProducts)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(cartProduct.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Produto com ID {cartProduct.ProductId} não encontrado.");

                var totalAmount = cartProduct.Quantity * product.Price;

                sale.Items.Add(new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = cartProduct.Quantity,
                    UnitPrice = product.Price,
                    Discount = 0, 
                    TotalAmount = totalAmount
                });

                sale.TotalAmount += totalAmount;
            }

            await _unitOfWork.SaleRepository.AddAsync(sale);
            await _unitOfWork.CommitAsync();

            var saleCreatedEvent = new SaleCreatedEvent(
                sale.Id,
                sale.SaleNumber,
                sale.SaleDate,
                sale.CustomerId,
                sale.TotalAmount
            );
            await _mediator.Publish(saleCreatedEvent);

            var saleDto = _mapper.Map<SaleDto>(sale);

            return ApiResponseDto<SaleDto>.AsCreated(saleDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<SaleDto>.AsInternalServerError($"Erro ao realizar checkout: {ex.Message}");
        }
    }
}
