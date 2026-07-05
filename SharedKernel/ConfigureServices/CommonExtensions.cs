using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel.Common.Wrappers.CurrentUser;
using SharedKernel.Middleware;
using SharedKernel.Shared;
using SharedKernel.Shared.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.ConfigureServices;

public static class CommonExtensions
{
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<ServiceEndpointOptions>(configuration);

        services.AddHttpClient<IBaseApiClient, BaseApiClient>();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        return services;
    }
}
