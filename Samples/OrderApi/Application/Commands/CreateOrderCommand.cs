using LevapTech.SimpleMediator.Abstractions;

public record CreateOrderCommand(string Product, int Quantity) : ICommand<Guid>;
