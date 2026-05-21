using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace ScrumPoker.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(Bootstrap).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(Bootstrap).Assembly);

        return services;
    }
}
