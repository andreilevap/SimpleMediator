using LevapTech.SimpleMediator.Abstractions;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<Order?>;
