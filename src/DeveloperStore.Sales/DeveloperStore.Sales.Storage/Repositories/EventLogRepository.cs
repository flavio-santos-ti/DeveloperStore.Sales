using DeveloperStore.Sales.Storage.EventLogging;
using DeveloperStore.Sales.Storage.Interfaces;
using MediatR;
using MongoDB.Driver;
using System.Text.Json;

namespace DeveloperStore.Sales.Storage.Repositories
{
    public class EventLogRepository : IEventLogRepository
    {
        private readonly IMongoCollection<EventLog> _logCollection;

        public EventLogRepository(IMongoDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Configurando a coleção
            _logCollection = context.GetCollection<EventLog>("EventLogs");
        }

        /// <summary>
        /// Registra um evento na coleção de logs.
        /// </summary>
        /// <typeparam name="TEvent">O tipo do evento que será registrado.</typeparam>
        /// <param name="eventInstance">A instância do evento.</param>
        public async Task LogAsync<TEvent>(TEvent eventInstance) where TEvent : INotification
        {
            if (eventInstance == null) throw new ArgumentNullException(nameof(eventInstance));

            var logEntry = new EventLog
            {
                EventName = typeof(TEvent).Name, // Nome do tipo do evento
                Timestamp = DateTime.UtcNow, // Data/hora do evento
                Data = JsonSerializer.Serialize(eventInstance) // Serializa o evento como JSON
            };

            await _logCollection.InsertOneAsync(logEntry); // Insere no MongoDB
        }
    }
}
