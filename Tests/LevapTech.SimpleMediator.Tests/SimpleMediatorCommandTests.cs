using Moq;
using LevapTech.SimpleMediator.Abstractions;

public class SampleCommand : ICommand<int> { }

public class SimpleMediatorCommandTests
{
    [Fact]
    public async Task SendAsync_CallsCommandHandler_ReturnsResult()
    {
        // Arrange
        var command = new SampleCommand();
        var expectedResult = 123;
        var handlerMock = new Mock<ICommandHandler<SampleCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResult);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand, int>)))
                           .Returns(handlerMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<SampleCommand, int>>)))
                           .Returns(Array.Empty<IPipelineBehavior<SampleCommand, int>>());

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act
        var result = await mediator.SendAsync<SampleCommand, int>(command);

        // Assert
        Assert.Equal(expectedResult, result);
        handlerMock.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new SampleCommand();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand, int>)))
                           .Returns(null);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<SampleCommand, int>>)))
                           .Returns(Array.Empty<IPipelineBehavior<SampleCommand, int>>());

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<SampleCommand, int>(command));
    }

    [Fact]
    public async Task SendAsync_HandlerThrows_ExceptionPropagates()
    {
        // Arrange
        var command = new SampleCommand();
        var handlerMock = new Mock<ICommandHandler<SampleCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new ApplicationException("Handler failed"));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand, int>)))
                           .Returns(handlerMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<SampleCommand, int>>)))
                           .Returns(Array.Empty<IPipelineBehavior<SampleCommand, int>>());

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ApplicationException>(() =>
            mediator.SendAsync<SampleCommand, int>(command));
    }

    [Fact]
    public async Task SendAsync_WithPipelineBehavior_InvokesBehavior()
    {
        // Arrange
        var command = new SampleCommand();
        var expectedResult = 42;

        var handlerMock = new Mock<ICommandHandler<SampleCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResult);

        var behaviorMock = new Mock<IPipelineBehavior<SampleCommand, int>>();
        behaviorMock.Setup(b => b.HandleAsync(command, It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
                    .Returns((SampleCommand cmd, CancellationToken ct, Func<Task<int>> next) => next());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand, int>)))
                           .Returns(handlerMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<SampleCommand, int>>)))
                           .Returns(new[] { behaviorMock.Object });

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act
        var result = await mediator.SendAsync<SampleCommand, int>(command);

        // Assert
        Assert.Equal(expectedResult, result);
        behaviorMock.Verify(b => b.HandleAsync(command, It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_PassesCancellationToken()
    {
        // Arrange
        var command = new SampleCommand();
        var expectedResult = 99;
        var cancellationToken = new CancellationTokenSource().Token;

        var handlerMock = new Mock<ICommandHandler<SampleCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(command, cancellationToken))
                   .ReturnsAsync(expectedResult);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICommandHandler<SampleCommand, int>)))
                           .Returns(handlerMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IPipelineBehavior<SampleCommand, int>>)))
                           .Returns(Array.Empty<IPipelineBehavior<SampleCommand, int>>());

        var mediator = new SimpleMediator(serviceProviderMock.Object);

        // Act
        var result = await mediator.SendAsync<SampleCommand, int>(command, cancellationToken);

        // Assert
        Assert.Equal(expectedResult, result);
        handlerMock.Verify(h => h.HandleAsync(command, cancellationToken), Times.Once);
    }

}
