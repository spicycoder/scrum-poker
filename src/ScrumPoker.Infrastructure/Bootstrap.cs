using Microsoft.Extensions.DependencyInjection;
using ScrumPoker.Infrastructure.Realtime;

namespace ScrumPoker.Infrastructure;

public static class Bootstrap
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
            options.KeepAliveInterval = TimeSpan.FromSeconds(2);
        });
        services.AddSingleton<PlayerConnectionTracker>();
        services.AddSingleton<PokerHubEventHandler>();

        return services;
    }
}
