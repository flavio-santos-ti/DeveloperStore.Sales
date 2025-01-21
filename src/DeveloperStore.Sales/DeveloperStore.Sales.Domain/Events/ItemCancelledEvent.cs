namespace DeveloperStore.Sales.Domain.Events;

public class ItemCancelledEvent
{
    public int SaleId { get; }
    public int SaleItemId { get; }
    public int ProductId { get; }
    public int QuantityCancelled { get; }

    public ItemCancelledEvent(int saleId, int saleItemId, int productId, int quantityCancelled)
    {
        SaleId = saleId;
        SaleItemId = saleItemId;
        ProductId = productId;
        QuantityCancelled = quantityCancelled;
    }
}
