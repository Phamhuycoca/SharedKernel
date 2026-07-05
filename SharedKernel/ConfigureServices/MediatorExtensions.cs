using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.ConfigureServices;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddAutoMapper(
            assembly,
            Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
