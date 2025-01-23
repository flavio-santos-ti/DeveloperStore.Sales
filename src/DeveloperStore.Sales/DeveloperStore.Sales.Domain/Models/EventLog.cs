namespace DeveloperStore.Sales.Domain.Models;

public class EventLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // ID único
    public string EventName { get; set; } = string.Empty; // Nome do evento
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Data/hora do log
    public object Data { get; set; } = null!; // Os dados do evento, armazenados como objeto
}
