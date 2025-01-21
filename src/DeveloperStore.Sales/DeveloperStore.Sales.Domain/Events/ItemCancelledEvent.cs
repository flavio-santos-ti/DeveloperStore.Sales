using MediatR;

namespace DeveloperStore.Sales.Domain.Events;

public class ItemCancelledEvent : INotification
{
    public int SaleId { get; }
    public string SaleNumber { get; }
    public int ItemId { get; }
    public int ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal TotalAmount { get; }
    public DateTime CancelledDate { get; }

    public ItemCancelledEvent(int saleId, string saleNumber, int itemId, int productId, int quantity, decimal unitPrice, decimal totalAmount, DateTime cancelledDate)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        ItemId = itemId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = totalAmount;
        CancelledDate = cancelledDate;
    }
}
