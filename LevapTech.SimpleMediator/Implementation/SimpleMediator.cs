using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public class SimpleMediator : ISimpleMediator
{
    private readonly IServiceProvider _serviceProvider;

    public SimpleMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await ExecutePipeline<TCommand, TResult>(command, cancellationToken, () => handler.HandleAsync(command, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await ExecutePipeline<TQuery, TResult>(query, cancellationToken, () => handler.HandleAsync(query, cancellationToken));
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }

    private async Task<TResult> ExecutePipeline<TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResult>> handler)
    {
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResult>>().Reverse().ToList();
        Func<Task<TResult>> next = handler;
        foreach (var behavior in behaviors)
        {
            var current = next;
            next = () => behavior.HandleAsync(request, cancellationToken, current);
        }
        return await next();
    }
}
