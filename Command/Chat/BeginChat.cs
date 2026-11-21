using MongoDB.Driver;
using OpenAI.Chat;
using Mediator.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KnowledgeCollector.Intelligence.Services.Interfaces;
using Data;
using Data.Mappers;

namespace Command.Chat;

public class BeginChat : ICommand<ObjectId>
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set;} = Guid.Empty;
    
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class BeginChatHandler : ICommandHandler<BeginChat, ObjectId>
{
    private IMongoDatabase _db {get;}
    private IAIChatService _aiChat {get;}

    public BeginChatHandler(IMongoDatabase db, IAIChatService aiChat)
    {
        _db = db;
        _aiChat = aiChat;
    }

    public async Task<ObjectId> Handle(BeginChat command, CancellationToken cancellationToken)
    {
        if (!command.Id.Equals(ObjectId.Empty)) 
            throw new Exception("A chat cannot be restarted. Please use BeginChat without an ObjectId");
        
        var configuration = await _db.GetCollection<Configuration>("configurations")
            .Find(x => x.Name.Equals("AgentInstruction")).FirstOrDefaultAsync(cancellationToken);

        if (configuration is null) throw new ArgumentNullException("Configuration: \"AgentInstruction\" is required. Ensure the database includes this.");

        var instruction = configuration.Value;

        List<ChatMessage> messages = [new SystemChatMessage(instruction)];

        command.Messages = messages;

        var history = await _aiChat.SendMessageAsync(command.Messages);

        command.Messages = history;

        var session = new Session
        {
            UserId = command.UserId,
            Messages = history.Select(SessionMapper.ToStored).ToList()
        };

        await _db.GetCollection<Session>("sessions")
            .InsertOneAsync(session);

        return session.Id;
    }
}
