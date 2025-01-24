using DeveloperStore.Sales.Domain.Events;
using DeveloperStore.Sales.Storage.UnitOfWork;
using MediatR;

namespace DeveloperStore.Sales.Service.Handlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public ItemCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        await _unitOfWork.EventLogRepository.LogAsync(notification); // Registra o evento no MongoDB
    }
}
