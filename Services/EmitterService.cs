using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tree_form_API.Models;

public class EmitterService
{
    private readonly IMongoCollection<Emitter> _emitterCollection;

    public EmitterService(IOptions<EmitterDatabaseSettings> emitterDatabaseSettings)
    {
        var mongoClient = new MongoClient(emitterDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(emitterDatabaseSettings.Value.DatabaseName);
        _emitterCollection = mongoDatabase.GetCollection<Emitter>(emitterDatabaseSettings.Value.CollectionName);
    }

    public async Task CreateAsync(Emitter? newEmitter)
    {
        if (newEmitter == null)
        {
            throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
        }

        await _emitterCollection.InsertOneAsync(newEmitter);
    }
}
