using Microsoft.Extensions.DependencyInjection;
using ScrumPoker.Infrastructure.Realtime;

namespace ScrumPoker.Infrastructure;

public static class Bootstrap
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? redisConnectionString = null)
    {
        var signalR = services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
            options.KeepAliveInterval = TimeSpan.FromSeconds(2);
        });

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            signalR.AddStackExchangeRedis(redisConnectionString);
        }

        services.AddSingleton<PlayerConnectionTracker>();
        services.AddSingleton<PokerHubEventHandler>();

        return services;
    }
}
