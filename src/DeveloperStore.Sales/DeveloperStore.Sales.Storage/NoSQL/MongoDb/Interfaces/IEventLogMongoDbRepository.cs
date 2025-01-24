using MediatR;

namespace DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces
{
    public interface IEventLogMongoDbRepository
    {
        /// <summary>
        /// Registra um evento na coleção de logs.
        /// </summary>
        /// <typeparam name="TEvent">O tipo do evento que será registrado.</typeparam>
        /// <param name="eventInstance">A instância do evento.</param>
        Task LogAsync<TEvent>(TEvent eventInstance) where TEvent : INotification;
    }
}
