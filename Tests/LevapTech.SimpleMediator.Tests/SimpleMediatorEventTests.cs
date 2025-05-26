using Moq;
using LevapTech.SimpleMediator.Abstractions;

public class SampleEvent : IEvent { }

public class SimpleMediatorEventTests
{
    [Fact]
    public async Task PublishAsync_CallsAllEventHandlers()
    {
        // Arrange
        var @event = new SampleEvent();
        var handlerMock1 = new Mock<IEventHandler<SampleEvent>>();
        var handlerMock2 = new Mock<IEventHandler<SampleEvent>>();

        var handlers = new List<IEventHandler<SampleEvent>> { handlerMock1.Object, handlerMock2.Object };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<SampleEvent>>)))
                           .Returns(handlers);

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act
        await mediator.PublishAsync(@event);

        // Assert
        handlerMock1.Verify(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        handlerMock2.Verify(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_NoHandlers_DoesNotThrow()
    {
        // Arrange
        var @event = new SampleEvent();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<SampleEvent>>)))
                           .Returns(Array.Empty<IEventHandler<SampleEvent>>());

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act & Assert
        await mediator.PublishAsync(@event); // Should not throw
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_ThrowsException()
    {
        // Arrange
        var @event = new SampleEvent();
        var handlerMock = new Mock<IEventHandler<SampleEvent>>();
        handlerMock.Setup(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Handler failed"));

        var handlers = new List<IEventHandler<SampleEvent>> { handlerMock.Object };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<SampleEvent>>)))
                           .Returns(handlers);

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.PublishAsync(@event));
    }

    [Fact]
    public async Task PublishAsync_CancellationToken_IsPassedToHandlers()
    {
        // Arrange
        var @event = new SampleEvent();
        var cancellationToken = new CancellationToken(true);

        var handlerMock = new Mock<IEventHandler<SampleEvent>>();
        handlerMock.Setup(h => h.HandleAsync(@event, cancellationToken))
                   .Returns(Task.CompletedTask)
                   .Verifiable();

        var handlers = new List<IEventHandler<SampleEvent>> { handlerMock.Object };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IEventHandler<SampleEvent>>)))
                           .Returns(handlers);

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act
        await mediator.PublishAsync(@event, cancellationToken);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(@event, cancellationToken), Times.Once);
    }
}