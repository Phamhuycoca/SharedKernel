using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Behaviors;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrapper.Exceptions;
using System.Net;
using System.Text.RegularExpressions;

namespace SharedKernel.Common.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;  

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;


        LogException(exception, traceId, httpContext);
        var (statusCode, message, errors) = MapException(exception);

        httpContext.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            status_code = (int)statusCode,
            message = message,
            instance = httpContext.Request.Path,
            errors = errors,
            trace_id = traceId,
        };
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    // =========================================
    // MAP EXCEPTION
    // =========================================

    private (HttpStatusCode statusCode, string message, object? errors)
        MapException(Exception ex)
    {
        return ex switch
        {
            DbUpdateException dbEx
                when dbEx.InnerException is SqlException sqlEx
                && (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                => (
                    HttpStatusCode.Conflict,  
                    ResponseMessage.AlreadyExists,
                    GetDuplicateErrors(sqlEx.Message)
                ),

            RequestValidationException fluentEx => (
                HttpStatusCode.UnprocessableEntity,
               ResponseMessage.Validator,
                fluentEx.Errors
            ),

            AppException appEx => (
                appEx.status_code,
                appEx.Message,
                appEx.detail != null
                    ? new { detail = appEx.detail }
                    : null
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ResponseMessage.Unauthorized,
                null
            ),

            KeyNotFoundException => ( 
                HttpStatusCode.NotFound,
                ResponseMessage.NotFound,
                null
            ),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                _env.IsDevelopment() ? argEx.Message : "Invalid request.",
                null
            ),

            OperationCanceledException => ( 
                HttpStatusCode.BadRequest,
                "Request was cancelled.",
                null
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                _env.IsDevelopment() ? ex.Message : ResponseMessage.InternalError,
                _env.IsDevelopment() ? (object?)new { stack_trace = ex.StackTrace } : null
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
        switch (exception)
        {
            case AppException appEx:
                _logger.LogWarning(
                    exception,
                    "Business exception | TraceId: {TraceId} | StatusCode: {StatusCode} | Method: {Method} | Path: {Path} | Message: {Message}",
                    traceId,
                    (int)appEx.status_code,
                    context.Request.Method,
                    context.Request.Path,
                    appEx.Message);
                break;

            case RequestValidationException validationEx:
                _logger.LogWarning(
                    exception,
                    "Validation failed | TraceId: {TraceId} | Method: {Method} | Path: {Path} | Message: {Message}",
                    traceId,
                    context.Request.Method,
                    context.Request.Path,
                    validationEx.Message);
                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception | TraceId: {TraceId} | Method: {Method} | Path: {Path}",
                    traceId,
                    context.Request.Method,
                    context.Request.Path);
                break;
        }
    }

    // =========================================
    // HELPERS
    // =========================================

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
