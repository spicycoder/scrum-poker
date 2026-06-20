using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace ScrumPoker.Application;

public static class Bootstrap
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Assembly? extraHandlerAssembly = null)
    {
        services.AddWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(Bootstrap).Assembly);
            if (extraHandlerAssembly is not null)
            {
                opts.Discovery.IncludeAssembly(extraHandlerAssembly);
            }
        });

        services.AddValidatorsFromAssembly(typeof(Bootstrap).Assembly);

        return services;
    }
}
