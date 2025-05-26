using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SimpleMediatorPipelineBehaviorTests
{
    public record TestCommand(int Value) : ICommand<int>;

    [Fact]
    public async Task SendAsync_NoPipelineBehaviors_CallsHandlerDirectly()
    {
        var handlerMock = new Mock<ICommandHandler<TestCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        var result = await mediator.SendAsync<TestCommand, int>(new TestCommand(1));

        Assert.Equal(42, result);
        handlerMock.Verify(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_SinglePipelineBehavior_InvokesBehavior()
    {
        var handlerMock = new Mock<ICommandHandler<TestCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var behaviorMock = new Mock<IPipelineBehavior<TestCommand, int>>();
        behaviorMock.Setup(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
            .Returns<TestCommand, CancellationToken, Func<Task<int>>>((cmd, ct, next) => next());

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(behaviorMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        var result = await mediator.SendAsync<TestCommand, int>(new TestCommand(2));

        Assert.Equal(10, result);
        behaviorMock.Verify(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()), Times.Once);
        handlerMock.Verify(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_MultiplePipelineBehaviors_InvokesInOrder()
    {
        var callOrder = new List<string>();

        var handlerMock = new Mock<ICommandHandler<TestCommand, int>>();
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5)
            .Callback(() => callOrder.Add("handler"));

        var behavior1 = new Mock<IPipelineBehavior<TestCommand, int>>();
        behavior1.Setup(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
            .Returns<TestCommand, CancellationToken, Func<Task<int>>>((cmd, ct, next) =>
            {
                callOrder.Add("behavior1");
                return next();
            });

        var behavior2 = new Mock<IPipelineBehavior<TestCommand, int>>();
        behavior2.Setup(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
            .Returns<TestCommand, CancellationToken, Func<Task<int>>>((cmd, ct, next) =>
            {
                callOrder.Add("behavior2");
                return next();
            });

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton<IPipelineBehavior<TestCommand, int>>(behavior1.Object);
        services.AddSingleton<IPipelineBehavior<TestCommand, int>>(behavior2.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        await mediator.SendAsync<TestCommand, int>(new TestCommand(3));

        Assert.Equal(new[] { "behavior1", "behavior2", "handler" }, callOrder);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorShortCircuits_HandlerNotCalled()
    {
        var handlerMock = new Mock<ICommandHandler<TestCommand, int>>();

        var behaviorMock = new Mock<IPipelineBehavior<TestCommand, int>>();
        behaviorMock.Setup(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
            .ReturnsAsync(99); // Does not call next()

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(behaviorMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        var result = await mediator.SendAsync<TestCommand, int>(new TestCommand(4));

        Assert.Equal(99, result);
        handlerMock.Verify(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorThrows_ExceptionPropagates()
    {
        var handlerMock = new Mock<ICommandHandler<TestCommand, int>>();

        var behaviorMock = new Mock<IPipelineBehavior<TestCommand, int>>();
        behaviorMock.Setup(b => b.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>(), It.IsAny<Func<Task<int>>>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(behaviorMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.SendAsync<TestCommand, int>(new TestCommand(5)));
    }
}
