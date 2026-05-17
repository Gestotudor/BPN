using FluentValidation;
using MediatR;
using TransferService.Application.Common.Results;

namespace TransferService.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(x => x.ValidateAsync(context, cancellationToken)));
        var failures = validationResults
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => x.ErrorMessage)
            .Distinct()
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(ResultErrorType.Validation, failures);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.Failure), new[] { typeof(ResultErrorType), typeof(string[]) });

            if (failureMethod is not null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { ResultErrorType.Validation, failures })!;
            }
        }

        throw new ValidationException(string.Join("; ", failures));
    }
}
