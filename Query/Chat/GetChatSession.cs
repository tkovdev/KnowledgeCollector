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
}

public class GetChatSessionHandler : IQueryHandler<GetChatSession, Session>
{
    private IMongoDatabase _db {get;}
    public GetChatSessionHandler(IMongoDatabase db)
    {
        _db = db;
    }

    // public override async Task<IList<IQueryResult>> GetAsync(Guid userId)
    // {
    //     var sessions = await _db.GetCollection<Session>("sessions")
    //         .Find(session => session.UserId == userId)
    //         .ToListAsync();

    //     return sessions.Cast<IQueryResult>().ToList();
    // }

    public async Task<Session> Handle(GetChatSession query, CancellationToken cancellationToken)
    {
        return await _db.GetCollection<Session>("sessions")
            .Find(session => session.Id == query.Id)
            .FirstOrDefaultAsync();
    }
}
