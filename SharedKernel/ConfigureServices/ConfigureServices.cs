using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedKernel.Behaviors;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrappers.CurrentUser;
using SharedKernel.Common.Wrappers.ErrorResponse;
using SharedKernel.Middleware;
using SharedKernel.Settings;
using SharedKernel.Shared;
using SharedKernel.Shared.ApiClient;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.ConfigureServices;


public static class ConfigureServices
{
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly,
        string jwtSectionName = "JwtSetting",
        string titleApp = "APP API",
        bool isUseSwagger = true,
        bool isUseJwtAuth = true,
        string versionApp = "v1")
    {
        if (isUseSwagger)
        {
            services.AddSwagger(titleApp, versionApp);
        }

        if (isUseJwtAuth)
        {
            services.AddJwtAuthentication(configuration, jwtSectionName);
        }

        services.AddMediator(assembly);
        services.AddValidation(assembly);
        services.AddCommonServices(configuration);

        return services;
    }
}

//public static class ConfigureServices
//{
//    public static IServiceCollection AddSharedKernel(
//        this IServiceCollection services,
//        IConfiguration configuration,
//        Assembly assembly,
//        string jwtSectionName = "JwtSetting",
//        string titleApp = "APP API",
//        bool isUseSwagger = true,
//        bool isUseJwtAuth = true,
//        string versionApp = "v1")
//    {
//        var JwtSetting = configuration
//           .GetSection(jwtSectionName)
//           .Get<JwtSetting>()
//           ?? throw new InvalidOperationException(
//               $"Missing or invalid configuration section: '{jwtSectionName}'. " +
//               $"Please check your appsettings.json.");

//        services.Configure<JwtSetting>(
//            configuration.GetSection(jwtSectionName));

//        // Swagger
//        services.AddEndpointsApiExplorer();
//        if (isUseSwagger)
//        {
//            services.AddSwaggerGen(options =>
//            {
//                options.SwaggerDoc(versionApp, new OpenApiInfo
//                {
//                    Title = titleApp,
//                    Version = versionApp
//                });

//                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//                {
//                    Name = "Authorization",
//                    In = ParameterLocation.Header,
//                    Type = SecuritySchemeType.Http,
//                    Scheme = "bearer",
//                    BearerFormat = "JWT",
//                    Description = "Bearer {token}"
//                });

//                options.AddSecurityRequirement(new OpenApiSecurityRequirement
//            {
//                {
//                    new OpenApiSecurityScheme
//                    {
//                        Reference = new OpenApiReference
//                        {
//                            Type = ReferenceType.SecurityScheme,
//                            Id = "Bearer"
//                        }
//                    },
//                    Array.Empty<string>()
//                }
//            });
//            });
//        }
//        // JWT Auth
//        if (isUseJwtAuth)
//        {
//            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//           .AddJwtBearer(options =>
//           {
//               options.TokenValidationParameters = new TokenValidationParameters
//               {
//                   ValidateIssuer = true,
//                   ValidateAudience = true,
//                   ValidateLifetime = true,
//                   ValidateIssuerSigningKey = true,
//                   ValidIssuer = JwtSetting.Issuer,
//                   ValidAudience = JwtSetting.Audience,
//                   IssuerSigningKey = new SymmetricSecurityKey(
//                       Encoding.UTF8.GetBytes(JwtSetting.SecretKey)),
//                   ClockSkew = TimeSpan.Zero
//               };

//               options.Events = new JwtBearerEvents
//               {
//                   // 401 — chưa đăng nhập / token không hợp lệ
//                   OnChallenge = async context =>
//                   {
//                       context.HandleResponse();
//                       context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
//                       context.Response.ContentType = "application/json";
//                       await context.Response.WriteAsJsonAsync(new
//                       {
//                           status_code = (int)HttpStatusCode.Unauthorized,
//                           message = ResponseMessage.Unauthorized,
//                           trace_id = context.HttpContext.TraceIdentifier
//                       });
//                   },
//                   // 403 — đã đăng nhập nhưng không có quyền
//                   OnForbidden = async context =>
//                   {
//                       context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//                       context.Response.ContentType = "application/json";
//                       await context.Response.WriteAsJsonAsync(new
//                       {
//                           status_code = (int)HttpStatusCode.Forbidden,
//                           message = ResponseMessage.Forbidden,
//                           trace_id = context.HttpContext.TraceIdentifier
//                       });
//                   },
//                   // Hỗ trợ truyền token qua query string ?access_token=...
//                   // Dùng cho SignalR / WebSocket
//                   OnMessageReceived = context =>
//                   {
//                       var accessToken = context.Request.Query["access_token"].FirstOrDefault();

//                       if (!string.IsNullOrWhiteSpace(accessToken))
//                           context.Token = accessToken;

//                       return Task.CompletedTask;
//                   }
//               };
//           });
//            services.AddAuthorization();
//        }
//        services.AddExceptionHandler<GlobalExceptionHandler>();
//        services.AddProblemDetails();
//        services.Configure<ServiceEndpointOptions>(configuration);
//        services.AddHttpClient<IBaseApiClient, BaseApiClient>();
//        // Cấu hình AutoMapper và MediatR
//        services.AddAutoMapper(assembly);
//        services.AddMediatR(cfg =>
//        {
//            cfg.RegisterServicesFromAssembly(assembly);
//            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

//            // Thứ tự chạy: Logging -> Validation
//            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
//            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
//        });
//        // Cấu hình CurrentUser
//        services.AddScoped<ICurrentUser, CurrentUser>();
//        services.AddValidatorsFromAssembly(assembly);
//        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//        services.Configure<ApiBehaviorOptions>(options =>
//        {
//            options.InvalidModelStateResponseFactory = context =>
//            {
//                var errors = context.ModelState
//                    .Where(x => x.Value!.Errors.Count > 0)
//                    .ToDictionary(
//                        x => x.Key.Replace("data.", ""),
//                        x => x.Value!.Errors
//                            .Select(e => e.ErrorMessage)
//                            .ToArray());

//                return new BadRequestObjectResult(new ApiErrorResponse
//                {
//                    status_code = (int)HttpStatusCode.UnprocessableEntity,
//                    message = ResponseMessage.Validator,
//                    errors = errors,
//                    trace_id = context.HttpContext.TraceIdentifier,
//                    instance = context.HttpContext.Request.Path
//                });
//            };
//        });
//        return services;
//    }
//}