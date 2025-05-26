using LevapTech.SimpleMediator.Abstractions;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public static readonly List<Order> Orders = new();

    public Task<Guid> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Product = command.Product,
            Quantity = command.Quantity,
            CreatedAt = DateTime.UtcNow
        };
        Orders.Add(order);
        return Task.FromResult(order.Id);
    }
}
