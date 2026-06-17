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
public sealed class RequestValidationException : Exception
{
    public IDictionary<string, string[]> errors { get; }

    public RequestValidationException(
        IDictionary<string, string[]> errors)
    {
        errors = errors;
    }
}