using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
 (IEnumerable<IValidator<TRequest>> validators)
: IPipelineBehavior<TRequest, TResponse>
where TRequest : class
{
    public async Task<TResponse> Handle
        (TRequest request, RequestHandlerDelegate<TResponse> next,
         CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)))
                    .ConfigureAwait(false);

            var failures = validationResults
                .Where(r => r.Errors.Count > 0)
                .SelectMany(r => r.Errors)
                .ToList();

           
            if (failures.Count > 0)
            {
                var errors = failures
                    .GroupBy(x => x.PropertyName.Replace("data.", ""))
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                throw new RequestValidationException(errors);
            }
        }
        return await next().ConfigureAwait(false);
    }
}
//public sealed class RequestValidationException : Exception
//{
//    public IDictionary<string, string[]> Errors { get; }

//    public RequestValidationException(
//        IDictionary<string, string[]> errors)
//    {
//        Errors = errors;
//    }
//}
public sealed class RequestValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public RequestValidationException(
        IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }

    /// <summary>
    /// Throw lỗi validate cho 1 field duy nhất.
    /// Ví dụ: RequestValidationException.Throw("user_id", "user_id is required");
    /// </summary>
    public static void Throw(string field, string message)
    {
        throw new RequestValidationException(
            new Dictionary<string, string[]>
            {
                [field] = new[] { message }
            });
    }

    /// <summary>
    /// Throw lỗi validate cho 1 field với nhiều message.
    /// Ví dụ: RequestValidationException.Throw("email", new[] { "email is required", "email is invalid" });
    /// </summary>
    public static void Throw(string field, string[] messages)
    {
        throw new RequestValidationException(
            new Dictionary<string, string[]>
            {
                [field] = messages
            });
    }

    /// <summary>
    /// Throw lỗi validate cho nhiều field cùng lúc.
    /// Ví dụ:
    /// RequestValidationException.Throw(new Dictionary&lt;string, string[]&gt;
    /// {
    ///     ["user_id"] = new[] { "user_id is required" },
    ///     ["role_id"] = new[] { "role_id is required" }
    /// });
    /// </summary>
    public static void Throw(IDictionary<string, string[]> errors)
    {
        throw new RequestValidationException(errors);
    }
}