namespace LevapTech.SimpleMediator.Abstractions
{
    // Event handler (Pub-Sub)
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
