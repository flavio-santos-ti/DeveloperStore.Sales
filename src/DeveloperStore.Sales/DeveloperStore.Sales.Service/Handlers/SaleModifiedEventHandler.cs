using DeveloperStore.Sales.Domain.Events;
using DeveloperStore.Sales.Storage.Interfaces;
using MediatR;

namespace DeveloperStore.Sales.Service.Handlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaleModifiedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        await _unitOfWork.EventLogRepository.LogAsync(notification); // Registra o evento no MongoDB
    }
}
