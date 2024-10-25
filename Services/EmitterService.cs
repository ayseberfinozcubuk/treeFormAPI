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

    // Create a new Emitter
    public async Task CreateAsync(Emitter newEmitter)
    {
        if (newEmitter == null)
        {
            throw new ArgumentNullException(nameof(newEmitter), "Emitter cannot be null.");
        }

        await _emitterCollection.InsertOneAsync(newEmitter);
    }

    // Get all Emitters
    public async Task<List<Emitter>> GetAllAsync() =>
        await _emitterCollection.Find(_ => true).ToListAsync();

    // Get an Emitter by ID
    public async Task<Emitter?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        return await _emitterCollection.Find(filter).FirstOrDefaultAsync();
    }

    // Update an Emitter by ID
    public async Task UpdateAsync(Guid id, Emitter updatedEmitter)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        var result = await _emitterCollection.ReplaceOneAsync(filter, updatedEmitter);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }

    // Delete an Emitter by ID
    public async Task DeleteAsync(Guid id)
    {
        var filter = Builders<Emitter>.Filter.Eq(e => e.Id, id);
        var result = await _emitterCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
        {
            throw new InvalidOperationException($"Emitter with ID {id} not found.");
        }
    }
}
