using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedKernel.Behaviors;
using SharedKernel.Common.Config;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Logging;
using SharedKernel.Common.Middleware;
using SharedKernel.Common.Wrapper.CurrentUser;
using SharedKernel.Common.Wrapper.Exceptions;
using SharedKernel.Infrastructure;
using SharedKernel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel;

public static class ConfigureServices
{
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly,
        string jwtSectionName = "Jwt",
        string titleApp = "APP API",
        string versionApp = "v1")
    {
        // Jwt options
        var jwtOptions = configuration
            .GetSection(jwtSectionName)
            .Get<JwtOptions>()!;

        services.Configure<JwtOptions>(
            configuration.GetSection(jwtSectionName));
        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(versionApp, new OpenApiInfo
            {
                Title = titleApp,
                Version = versionApp
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // JWT Auth
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };

                options.Events = new JwtBearerEvents
                {
                    // 401 — chưa đăng nhập / token không hợp lệ
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            status_code = 401,
                            message = ResponseMessage.Unauthorized,
                            trace_id = context.HttpContext.TraceIdentifier
                        });
                    },

                    // 403 — đã đăng nhập nhưng không có quyền
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            status_code = 403,
                            message = ResponseMessage.Forbidden,
                            trace_id = context.HttpContext.TraceIdentifier
                        });
                    },

                    // Hỗ trợ truyền token qua query string ?access_token=...
                    // Dùng cho SignalR / WebSocket
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"]
                            .FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.Configure<ServiceEndpointOptions>(configuration);
        services.AddHttpClient<IBaseApiClient, BaseApiClient>();
        //Cau hinh AutoMapper va MediatR
        services.AddAutoMapper(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        // Cau hinh CurrentUser
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>),typeof(LoggingBehavior<,>));
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key.Replace("data.", ""),
                        x => x.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray());

                return new BadRequestObjectResult(new ApiErrorResponse
                {
                    status_code = (int)HttpStatusCode.UnprocessableEntity,
                    message = ResponseMessage.Validator,
                    errors = errors,
                    trace_id = context.HttpContext.TraceIdentifier,
                    instance = context.HttpContext.Request.Path
                });
            };
        });
        return services;
    }

}
