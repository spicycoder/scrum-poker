using Microsoft.Extensions.DependencyInjection;
using ScrumPoker.Domain.Abstractions;

namespace ScrumPoker.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddOptions<GameSettings>()
            .BindConfiguration("Game");

        return services;
    }
}
