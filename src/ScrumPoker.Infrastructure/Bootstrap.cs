using Microsoft.Extensions.DependencyInjection;

namespace ScrumPoker.Infrastructure;

public static class Bootstrap
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }
}
