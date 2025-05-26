# SimpleMediator

[![NuGet](https://img.shields.io/nuget/v/LevapTech.SimpleMediator.svg)](https://www.nuget.org/packages/LevapTech.SimpleMediator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)

A lightweight, dependency-injected mediator library for .NET, supporting CQRS, Pub-Sub, and pipeline behaviors. SimpleMediator helps you decouple your application logic by handling commands, queries, and events through a clean, extensible interface.

## Features

- **CQRS (Command Query Responsibility Segregation):** Separate command and query handling for clear, maintainable code.
- **Pub-Sub (Event Publishing):** Publish events to multiple handlers for robust event-driven architectures.
- **Dependency Injection:** Built-in support for Microsoft.Extensions.DependencyInjection.
- **Pipeline Behaviors:** Add cross-cutting concerns (logging, validation, etc.) via pipeline behaviors.

---

## Installation


```
dotnet add package LevapTech.SimpleMediator
```
---

## Getting Started

### 1. Register Services

Add SimpleMediator and your handlers to the DI container:


```csharp
using LevapTech.SimpleMediator;
using LevapTech.SimpleMediator.Abstractions;

var services = new ServiceCollection();

// Register mediator
services.AddSingleton<ISimpleMediator, SimpleMediator>();

// Register handlers
services.AddTransient<ICommandHandler<CreateOrderCommand, Guid>, CreateOrderCommandHandler>();
services.AddTransient<IQueryHandler<GetOrderByIdQuery, Order>, GetOrderByIdQueryHandler>();
services.AddTransient<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();

// Register pipeline behaviors (optional)
services.AddTransient<IPipelineBehavior<CreateOrderCommand, Guid>, LoggingBehavior<CreateOrderCommand, Guid>>();

```

---

### 2. Define Commands, Queries, and Events


```csharp
// Command
public record CreateOrderCommand(string Product, int Quantity) : ICommand<Guid>;

// Query
public record GetOrderByIdQuery(Guid OrderId) : IQuery<Order>;

// Event
public record OrderCreatedEvent(Guid OrderId) : IEvent;

```

---

### 3. Implement Handlers


```csharp
// Command Handler
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // ... create order logic ...
        return Task.FromResult(Guid.NewGuid());
    }
}

// Query Handler
public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Order>
{
    public Task<Order> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        // ... fetch order logic ...
        return Task.FromResult(new Order());
    }
}

// Event Handler
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // ... handle event ...
        return Task.CompletedTask;
    }
}

```

---

### 4. Use the Mediator


```csharp
public class OrdersController : ControllerBase
{
    private readonly ISimpleMediator _mediator;

    public OrdersController(ISimpleMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderCommand command)
    {
        var orderId = await _mediator.SendAsync<CreateOrderCommand, Guid>(command);
        await _mediator.PublishAsync(new OrderCreatedEvent(orderId));
        return Ok(orderId);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _mediator.QueryAsync<GetOrderByIdQuery, Order>(new GetOrderByIdQuery(id));
        return Ok(order);
    }
}

```

---

### 5. Add Pipeline Behaviors (Optional)

Pipeline behaviors allow you to add cross-cutting logic (e.g., logging, validation):


```csharp
public class LoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next)
    {
        Console.WriteLine($"Handling {typeof(TRequest).Name}");
        var result = await next();
        Console.WriteLine($"Handled {typeof(TRequest).Name}");
        return result;
    }
}

```

Register your behavior in DI as shown above.

---

## License

MIT
