namespace DeveloperStore.Sales.Domain.Events;


public class SaleCreatedEvent
{
    public int SaleId { get; }
    public string SaleNumber { get; }
    public DateTime SaleDate { get; }
    public int CustomerId { get; }
    public decimal TotalAmount { get; }

    public SaleCreatedEvent(int saleId, string saleNumber, DateTime saleDate, int customerId, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
