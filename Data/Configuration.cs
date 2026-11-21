using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Data;

public class Configuration
{
    [BsonId]
    public ObjectId Id { get; set; }
    public required string Name { get; set; }
    public required string Value { get; set; }

}