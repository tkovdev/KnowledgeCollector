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
    
    public List<SessionMessage> Messages { get; set; } = new List<SessionMessage>();
}

public class SessionMessage
{
    public string Role { get; set; } = "";
    public string? ParticipantName { get; set; }
    public List<string> TextParts { get; set; } = [];
    public List<SessionToolCall> ToolCalls { get; set; } = [];
    public string? ToolCallId { get; set; }
}

public class SessionToolCall
{
    public string Id { get; set; } = "";
    public string FunctionName { get; set; } = "";
    public string ArgumentsJson { get; set; } = "";
}