using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Entities;
using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using MediatR;
using System.Text.Json;

namespace DeveloperStore.Sales.Storage.NoSQL.MongoDb.Repositories;

public class EventLogMongoDbRepository : BaseMongoDbRepository<EventLog>, IEventLogMongoDbRepository
{
    public EventLogMongoDbRepository(IMongoDbContext context)
        : base(context, "EventLogs") // Define o nome da coleção como "EventLogs"
    {
    }

    public async Task LogAsync<TEvent>(TEvent eventInstance) where TEvent : INotification
    {
        if (eventInstance == null) throw new ArgumentNullException(nameof(eventInstance));

        // Monta o log específico para o evento
        var logEntry = new EventLog
        {
            EventName = typeof(TEvent).Name,
            Timestamp = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(eventInstance)
        };

        // Insere o log usando a funcionalidade da classe base
        await InsertAsync(logEntry);
    }
}
