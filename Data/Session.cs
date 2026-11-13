using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using OpenAI.Chat;

namespace Data;

public class Session
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set;}
    
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}