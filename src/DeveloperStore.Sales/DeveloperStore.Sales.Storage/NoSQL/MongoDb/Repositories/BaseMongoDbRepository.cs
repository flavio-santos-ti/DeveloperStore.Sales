using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using MongoDB.Driver;

namespace DeveloperStore.Sales.Storage.NoSQL.MongoDb.Repositories;

public abstract class BaseMongoDbRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    protected BaseMongoDbRepository(IMongoDbContext context, string collectionName)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _collection = context.GetCollection<T>(collectionName);
    }

    // Método genérico para inserir
    public async Task InsertAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await _collection.InsertOneAsync(entity);
    }

    // Método genérico para buscar por ID
    public async Task<T?> GetByIdAsync(string id)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();
    }

    // Método genérico para buscar todos
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    // Método genérico para deletar
    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
    }

    // Método protegido para obter a coleção diretamente (caso repositórios específicos precisem)
    protected IMongoCollection<T> Collection => _collection;
}
