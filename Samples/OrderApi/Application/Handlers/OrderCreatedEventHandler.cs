using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.Logging;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order created with ID: {OrderId}", @event.OrderId);
        return Task.CompletedTask;
    }
}
