using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class StoreDomain
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string StoreName { get; set; }
    
    public List<ItemDomain> Items { get; set; } // Reference to another collection
}
