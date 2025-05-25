namespace LevapTech.SimpleMediator.Abstractions
{
    // Pipeline behavior
    public interface IPipelineBehavior<TRequest, TResult>
    {
        Task<TResult> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResult>> next);
    }
}
