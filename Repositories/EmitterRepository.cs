using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

public class EmitterRepository
{
    private readonly IMongoCollection<BsonDocument> _emittersCollection;

    public EmitterRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["DatabaseName"]);
        _emittersCollection = database.GetCollection<BsonDocument>("Emitters");
    }

    public async Task CreateEmitterAsync(BsonDocument emitterDocument)
    {
        await _emittersCollection.InsertOneAsync(emitterDocument);
    }
}
