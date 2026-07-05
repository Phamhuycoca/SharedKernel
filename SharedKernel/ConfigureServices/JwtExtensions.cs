using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrappers.ErrorResponse;
using SharedKernel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.ConfigureServices;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
    {
        services.Configure<JwtSetting>(
            configuration.GetSection(sectionName));

        var jwtSetting = configuration
            .GetSection(sectionName)
            .Get<JwtSetting>()
            ?? throw new AppException(HttpStatusCode.BadRequest,
                $"Missing configuration: {sectionName}");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSetting.Issuer,
                    ValidAudience = jwtSetting.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSetting.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        return WriteError(
                            context.HttpContext,
                            HttpStatusCode.Unauthorized,
                            ResponseMessage.Unauthorized);
                    },

                    OnForbidden = context =>
                    {
                        return WriteError(
                            context.HttpContext,
                            HttpStatusCode.Forbidden,
                            ResponseMessage.Forbidden);
                    },

                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"].FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static async Task WriteError(
        HttpContext context,
        HttpStatusCode status,
        string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status_code = (int)status,
            message,
            trace_id = context.TraceIdentifier
        });
    }
}
