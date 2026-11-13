namespace Mediator.Interfaces;

public interface IMediator
{
    Task Send(ICommand command, CancellationToken cancellationToken = default);
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}