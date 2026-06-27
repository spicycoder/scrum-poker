using Microsoft.Extensions.DependencyInjection;
using ScrumPoker.Domain.Abstractions;
using ScrumPoker.Persistence.Stats;

namespace ScrumPoker.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IStatsRepository, StatsRepository>();
        services.AddHostedService<DatabaseInitializer>();
        services.AddOptions<GameSettings>()
            .BindConfiguration("Game");

        return services;
    }
}
