using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Behaviors;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrapper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedKernel.Common.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        // log
        LogException(exception, traceId, httpContext);

        // map exception
        var (statusCode, detail, errors)
            = MapException(exception);

        httpContext.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            status_code = (int)statusCode,
            message = detail,
            instance = httpContext.Request.Path,
            errors = errors,
            trace_id = traceId,
        };


        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }

    // =========================================
    // MAP EXCEPTION
    // =========================================

    private static (
        HttpStatusCode statusCode,
        string? detail,
        object? errors)
        MapException(Exception ex)
    {
        return ex switch
        {
            DbUpdateException dbEx
            when dbEx.InnerException is SqlException sqlEx
            && (sqlEx.Number == 2601 || sqlEx.Number == 2627)
            => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                GetDuplicateErrors(sqlEx.Message)
            ),

            RequestValidationException fluentEx => (
                HttpStatusCode.BadRequest,
                fluentEx.Message,
                fluentEx.errors
            ),

            AppException appEx => (
                appEx.status_code,
                appEx.Message,
                null
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ResponseMessage.Unauthorized,
                null
            ),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                ex.Message,
                null
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                IsDevelopment()
                    ? ex.Message
                    : ResponseMessage.InternalError,
                null
            )
        };
    }

    // =========================================
    // LOG
    // =========================================

    private void LogException(
        Exception exception,
        string traceId,
        HttpContext context)
    {
        if (exception is AppException appEx)
        {
            _logger.LogWarning(
                "Business exception | TraceId: {TraceId} | Message: {Message}",
                traceId,
                appEx.Message);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception | TraceId: {TraceId} | Method: {Method} | Path: {Path}",
                traceId,
                context.Request.Method,
                context.Request.Path);
        }
    }

    // =========================================
    // HELPERS
    // =========================================

    private static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT")
            == "Development";
    }
    private static Dictionary<string, string[]>? GetDuplicateErrors(string message)
    {
        var match = Regex.Match(
            message,
            @"(?:index|constraint)\s'([^']+)'",
            RegexOptions.IgnoreCase);

        if (!match.Success)
            return null;

        var constraintName = match.Groups[1].Value;

        if (constraintName.StartsWith("IX_"))
        {
            var parts = constraintName.Split('_');

            if (parts.Length >= 3)
            {
                var fieldName = string.Join("_", parts.Skip(2));

                return new Dictionary<string, string[]>
                {
                    [fieldName] = [$"{fieldName} already exists"]
                };
            }
        }

        if (constraintName.StartsWith("PK_"))
        {
            return new Dictionary<string, string[]>
            {
                ["id"] = ["Id already exists"]
            };
        }

        return null;
    }
}
