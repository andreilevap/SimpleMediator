namespace LevapTech.SimpleMediator.Abstractions
{
    public interface ISimpleMediator
    {
        Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResult>;

        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>;

        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent;
    }
}
