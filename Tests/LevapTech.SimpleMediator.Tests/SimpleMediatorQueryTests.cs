using Moq;
using Microsoft.Extensions.DependencyInjection;
using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class SimpleMediatorQueryTests
{
    public class TestQuery : IQuery<string> { }
    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> HandleAsync(TestQuery query, CancellationToken cancellationToken) =>
            Task.FromResult("result");
    }

    [Fact]
    public async Task QueryAsync_ReturnsExpectedResult()
    {
        // Arrange
        var query = new TestQuery();
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
                   .ReturnsAsync("expected");

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton<IPipelineBehavior<TestQuery, string>>(sp => new NoOpBehavior<TestQuery, string>());
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        // Act
        var result = await mediator.QueryAsync<TestQuery, string>(query);

        // Assert
        Assert.Equal("expected", result);
        handlerMock.Verify(h => h.HandleAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_Throws_WhenHandlerThrows()
    {
        // Arrange
        var query = new TestQuery();
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("fail"));

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mediator.QueryAsync<TestQuery, string>(query));
    }

    [Fact]
    public async Task QueryAsync_InvokesPipelineBehaviorsInOrder()
    {
        // Arrange
        var query = new TestQuery();
        var callOrder = new List<string>();

        var behavior1 = new Mock<IPipelineBehavior<TestQuery, string>>();
        behavior1.Setup(b => b.HandleAsync(query, It.IsAny<CancellationToken>(), It.IsAny<Func<Task<string>>>()))
            .Returns(async (TestQuery q, CancellationToken ct, Func<Task<string>> next) =>
            {
                callOrder.Add("behavior1");
                var result = await next();
                callOrder.Add("behavior1-after");
                return result;
            });

        var behavior2 = new Mock<IPipelineBehavior<TestQuery, string>>();
        behavior2.Setup(b => b.HandleAsync(query, It.IsAny<CancellationToken>(), It.IsAny<Func<Task<string>>>()))
            .Returns(async (TestQuery q, CancellationToken ct, Func<Task<string>> next) =>
            {
                callOrder.Add("behavior2");
                var result = await next();
                callOrder.Add("behavior2-after");
                return result;
            });

        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
                   .ReturnsAsync("pipeline");

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        services.AddSingleton(behavior1.Object);
        services.AddSingleton(behavior2.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        // Act
        var result = await mediator.QueryAsync<TestQuery, string>(query);

        // Assert
        Assert.Equal("pipeline", result);
        Assert.Equal(new[] { "behavior1", "behavior2", "behavior2-after", "behavior1-after" }, callOrder);
    }

    [Fact]
    public async Task QueryAsync_PassesCancellationToken()
    {
        // Arrange
        var query = new TestQuery();
        var cts = new CancellationTokenSource();
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock.Setup(h => h.HandleAsync(query, cts.Token))
                   .ReturnsAsync("cancel");

        var services = new ServiceCollection();
        services.AddSingleton(handlerMock.Object);
        var provider = services.BuildServiceProvider();

        var mediator = new SimpleMediator(provider);

        // Act
        var result = await mediator.QueryAsync<TestQuery, string>(query, cts.Token);

        // Assert
        Assert.Equal("cancel", result);
        handlerMock.Verify(h => h.HandleAsync(query, cts.Token), Times.Once);
    }

    // Helper no-op behavior
    private class NoOpBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
    {
        public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next)
            => next();
    }
}
