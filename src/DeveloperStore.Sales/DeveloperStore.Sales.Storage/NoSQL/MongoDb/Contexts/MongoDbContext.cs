using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeveloperStore.Sales.Storage.NoSQL.MongoDb.Contexts;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
        _database = client.GetDatabase("DeveloperStoreLogs");
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
