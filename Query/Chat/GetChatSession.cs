using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Mediator.Interfaces;
using Data;

namespace Query.Chat;

public class GetChatSession : IQuery<Session>
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid UserId { get; set; } = Guid.Empty;
}

public class GetChatSessionHandler : IQueryHandler<GetChatSession, Session>
{
    private IMongoDatabase _db {get;}
    public GetChatSessionHandler(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task<Session> Handle(GetChatSession query, CancellationToken cancellationToken)
    {
        return await _db.GetCollection<Session>("sessions")
            .Find(session => session.Id.Equals(query.Id) && session.UserId.Equals(query.UserId))
            .FirstOrDefaultAsync();
    }
}
