namespace LevapTech.SimpleMediator.Abstractions
{
    /// <summary>
    /// Defines the contract for a simple mediator to handle commands, queries, and events.
    /// </summary>
    public interface ISimpleMediator
    {
        /// <summary>
        /// Sends a command to its corresponding handler and returns a result asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to send.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
        /// <param name="command">The command instance to process.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, containing the result from the handler.</returns>
        Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResult>;

        /// <summary>
        /// Sends a query to its corresponding handler and returns a result asynchronously.
        /// </summary>
        /// <typeparam name="TQuery">The type of the query to send.</typeparam>
        /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
        /// <param name="query">The query instance to process.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, containing the result from the handler.</returns>
        Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResult>;

        /// <summary>
        /// Publishes an event to all registered event handlers asynchronously.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        /// <param name="event">The event instance to publish.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous publish operation.</returns>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent;
    }
}
