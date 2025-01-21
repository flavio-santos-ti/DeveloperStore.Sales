using DeveloperStore.Sales.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperStore.Sales.Service.Handlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    public async Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"Venda modificada: Número {notification.SaleNumber}, Cliente {notification.CustomerId}, Novo Total: {notification.NewTotalAmount}");
        });
    }
}