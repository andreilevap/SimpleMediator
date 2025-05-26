using LevapTech.SimpleMediator.Abstractions;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Order?>
{
    public Task<Order?> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = CreateOrderCommandHandler.Orders.FirstOrDefault(o => o.Id == query.OrderId);
        return Task.FromResult(order);
    }
}
