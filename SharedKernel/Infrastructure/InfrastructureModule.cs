using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure;
public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? dbOptions = null
    )
        where TContext : DbContext
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddDbContext<TContext>(options =>
        {
            if (dbOptions != null)
            {
                dbOptions(options);
            }
            else
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")
                );
            }
        });

        return services;
    }
}