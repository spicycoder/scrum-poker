using Microsoft.Extensions.DependencyInjection;
using ScrumPoker.Infrastructure.Realtime;

namespace ScrumPoker.Infrastructure;

public static class Bootstrap
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<PokerHubEventHandler>();

        return services;
    }
}
