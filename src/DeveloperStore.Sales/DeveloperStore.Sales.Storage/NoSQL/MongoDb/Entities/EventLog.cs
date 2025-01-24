using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeveloperStore.Sales.Storage.NoSQL.MongoDb.Entities;

public class EventLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("EventName")]
    public string EventName { get; set; } = string.Empty;

    [BsonElement("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("Data")]
    public object Data { get; set; } = null!;
}