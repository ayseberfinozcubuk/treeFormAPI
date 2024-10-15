using MongoDB.Driver;
using System.Threading.Tasks;

public class DataService
{
    private readonly IMongoCollection<EmiterNodePriDomain> _emiterNodePriCollection;
    private readonly IMongoCollection<StoreDomain> _storeCollection;
    private readonly IMongoCollection<ItemDomain> _itemCollection;

    public DataService(IMongoDatabase database)
    {
        _emiterNodePriCollection = database.GetCollection<EmiterNodePriDomain>("EmiterNodePriCollection");
        _storeCollection = database.GetCollection<StoreDomain>("StoreCollection");
        _itemCollection = database.GetCollection<ItemDomain>("ItemCollection");
    }

    public async Task InsertDataAsync(InputDataDto inputData)
    {
        // Transform input DTO into domain models
        var emiterNode = new EmiterNodePriDomain
        {
            Name = inputData.Name,
            Stores = inputData.Stores?.ConvertAll(s => new StoreDomain
            {
                StoreName = s.StoreName,
                Items = s.Items?.ConvertAll(i => new ItemDomain { ItemName = i.ItemName })
            })
        };

        // Insert data into respective collections
        foreach (var store in emiterNode.Stores)
        {
            await _storeCollection.InsertOneAsync(store);
            foreach (var item in store.Items)
            {
                await _itemCollection.InsertOneAsync(item);
            }
        }

        await _emiterNodePriCollection.InsertOneAsync(emiterNode);
    }
}
