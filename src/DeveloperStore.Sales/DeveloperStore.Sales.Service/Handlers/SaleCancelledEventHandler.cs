using DeveloperStore.Sales.Domain.Events;
using MediatR;

namespace DeveloperStore.Sales.Service.Handlers;

public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    public async Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {

        await Task.Run(() =>
        {
            Console.WriteLine($"Venda cancelada: {notification.SaleNumber}, Cliente: {notification.CustomerId}, Data do Cancelamento: {notification.CancelledDate}");
        });
    }
}
