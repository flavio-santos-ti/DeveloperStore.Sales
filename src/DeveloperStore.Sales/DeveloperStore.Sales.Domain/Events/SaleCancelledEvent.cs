using MediatR;

namespace DeveloperStore.Sales.Domain.Events
{
    public class SaleCancelledEvent : INotification
    {
        public int SaleId { get; }
        public string SaleNumber { get; }
        public DateTime SaleDate { get; }
        public int CustomerId { get; }
        public DateTime CancelledDate { get; }
        public decimal TotalAmount { get; } 

        public SaleCancelledEvent(int saleId, string saleNumber, DateTime saleDate, DateTime cancelledDate, int customerId, decimal totalAmount)
        {
            SaleId = saleId;
            SaleNumber = saleNumber;
            SaleDate = saleDate;
            CancelledDate = cancelledDate;
            CustomerId = customerId;
            TotalAmount = totalAmount; 
        }
    }
}
