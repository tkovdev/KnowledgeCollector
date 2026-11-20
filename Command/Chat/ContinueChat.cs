using MongoDB.Driver;
using OpenAI.Chat;
using Mediator.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using KnowledgeCollector.Intelligence.Services.Interfaces;
using Data;
using Data.Mappers;

namespace Command.Chat;

public class ContinueChat : ICommand<ObjectId>
{
    public ContinueChat(){}
    public ContinueChat(string message)
    {
        Message = message;    
    }

    public ContinueChat(string sessionId, string message)
    {
        Id = ObjectId.Parse(sessionId);
        Message = message;
    }

    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set;} = Guid.Empty;
    
    public string Message { get; set; } = string.Empty;
}

public class ContinueChatHandler : ICommandHandler<ContinueChat, ObjectId>
{
    private IMongoDatabase _db {get;}
    private IAIChatService _aiChat {get;}

    public ContinueChatHandler(IMongoDatabase db, IAIChatService aiChat)
    {
        _db = db;
        _aiChat = aiChat;
    }

    public async Task<ObjectId> Handle(ContinueChat command, CancellationToken cancellationToken)
    {
        if (command.Id.Equals(ObjectId.Empty))
            throw new Exception("Only an existing chat can be continued. Please use BeginChat and provide the session id from the resulting call.");

        var sessions = _db.GetCollection<Session>("sessions");
        var session = await sessions
            .Find(x => x.Id == command.Id && x.UserId == command.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            throw new KeyNotFoundException("Chat session not found.");

        var history = session.Messages
            .Select(SessionMapper.ToSdk)
            .ToList();

        history.Add(new UserChatMessage(command.Message));

        var updatedHistory = await _aiChat.SendMessageAsync(history);

        session.Messages = updatedHistory
            .Select(SessionMapper.ToStored)
            .ToList();

        await sessions.ReplaceOneAsync(
            x => x.Id == session.Id && x.UserId == command.UserId,
            session,
            cancellationToken: cancellationToken);

        return session.Id;
    }
}
