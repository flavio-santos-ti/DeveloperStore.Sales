using MediatR;

namespace DeveloperStore.Sales.Domain.Events;

public class SaleModifiedEvent : INotification
{
    public int SaleId { get; }
    public string SaleNumber { get; }
    public DateTime ModifiedDate { get; }
    public int CustomerId { get; }
    public decimal NewTotalAmount { get; }

    public SaleModifiedEvent(int saleId, string saleNumber, DateTime modifiedDate, int customerId, decimal newTotalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        ModifiedDate = modifiedDate;
        CustomerId = customerId;
        NewTotalAmount = newTotalAmount;
    }
}
