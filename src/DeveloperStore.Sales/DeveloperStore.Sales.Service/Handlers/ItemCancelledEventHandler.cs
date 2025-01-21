using DeveloperStore.Sales.Domain.Events;
using MediatR;

namespace DeveloperStore.Sales.Service.Handlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    public async Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"Item cancelado: Produto {notification.ProductId}, Quantidade {notification.Quantity}, Venda {notification.SaleId}");
        });
    }
}
