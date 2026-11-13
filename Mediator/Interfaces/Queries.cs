using MongoDB.Bson;

namespace Mediator.Interfaces;

public interface IQueryResult;

public interface IQuery<TResult>;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}