namespace DeveloperStore.Sales.Domain.Events;

public class SaleModifiedEvent
{
    public int SaleId { get; }
    public DateTime ModifiedDate { get; }
    public decimal NewTotalAmount { get; }

    public SaleModifiedEvent(int saleId, DateTime modifiedDate, decimal newTotalAmount)
    {
        SaleId = saleId;
        ModifiedDate = modifiedDate;
        NewTotalAmount = newTotalAmount;
    }
}
