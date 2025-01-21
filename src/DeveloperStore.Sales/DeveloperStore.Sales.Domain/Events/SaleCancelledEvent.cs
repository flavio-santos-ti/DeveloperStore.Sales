namespace DeveloperStore.Sales.Domain.Events;

public class SaleCancelledEvent
{
    public int SaleId { get; }
    public string SaleNumber { get; }
    public DateTime CancelledDate { get; }

    public SaleCancelledEvent(int saleId, string saleNumber, DateTime cancelledDate)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        CancelledDate = cancelledDate;
    }
}
