using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace ScrumPoker.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] extraHandlerAssemblies)
    {
        services.AddWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(Bootstrap).Assembly);
            foreach (var assembly in extraHandlerAssemblies)
            {
                opts.Discovery.IncludeAssembly(assembly);
            }
        });

        services.AddValidatorsFromAssembly(typeof(Bootstrap).Assembly);

        return services;
    }
}
