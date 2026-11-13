using MongoDB.Driver;
using OpenAI.Chat;
using Mediator.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KnowledgeCollector.Intelligence.Services.Interfaces;

namespace Command.Chat;

public class BeginChat : ICommand
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set;} = Guid.Empty;
    
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class BeginChatHandler : ICommandHandler<BeginChat>
{
    private IMongoDatabase _db {get;}
    private IAIChatService _aiChat {get;}

    public BeginChatHandler(IMongoDatabase db, IAIChatService aiChat)
    {
        _db = db;
        _aiChat = aiChat;
    }

    public async Task Handle(BeginChat command, CancellationToken cancellationToken)
    {
        if (!command.Id.Equals(ObjectId.Empty)) 
            throw new Exception("A chat cannot be restarted. Please use BeginChat without an ObjectId");
        

        var instruction = """
            You are tasked with creating one valuable question to ask the user regarding their team, to uncover processes and elicit information that will later be used to generate a quiz for others to assess their knowledge on the user's team.
        """;
        List<ChatMessage> messages = [new SystemChatMessage(instruction)];

        command.Messages = messages;

        var response = await _aiChat.SendMessageAsync(command.Messages);

        command.Messages = response;

        await _db.GetCollection<BeginChat>("sessions")
            .InsertOneAsync(command);

        // return beginChatOptions.Session;
        return;
    }
}
