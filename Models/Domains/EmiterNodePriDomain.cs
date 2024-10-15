using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EmiterNodePriDomain
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public List<StoreDomain> Stores { get; set; } // Reference to another collection
}
