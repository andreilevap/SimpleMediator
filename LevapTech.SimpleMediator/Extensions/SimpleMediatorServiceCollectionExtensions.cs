using LevapTech.SimpleMediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class SimpleMediatorServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services)
    {
        services.AddScoped<ISimpleMediator, SimpleMediator>();
        return services;
    }
}
