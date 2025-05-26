using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.Logging;

public class LoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResult>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResult>> logger)
    {
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken, Func<Task<TResult>> next)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var result = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return result;
    }
}
