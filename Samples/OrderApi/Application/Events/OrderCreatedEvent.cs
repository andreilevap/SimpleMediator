using LevapTech.SimpleMediator.Abstractions;

public record OrderCreatedEvent(Guid OrderId) : IEvent;
