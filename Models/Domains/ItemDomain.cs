using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ItemDomain
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string ItemName { get; set; }
}
