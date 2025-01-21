using DeveloperStore.Sales.Domain.Events;
using MediatR;

namespace DeveloperStore.Sales.Service.Handlers;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    public async Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"Venda criada: {notification.SaleNumber} com total de {notification.TotalAmount}");
        });
    }
}
