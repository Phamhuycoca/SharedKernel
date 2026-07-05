using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrappers.ErrorResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.ConfigureServices;

public static class ValidationExtensions
{
    private const string Prefix = "data.";

    public static IServiceCollection AddValidation(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .ToDictionary(
                        x => x.Key.Replace(Prefix, ""),
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