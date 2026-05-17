using AuthService.Application.Common.Results;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Behaviors;

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
        var validationResults = await Task.WhenAll(
            _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .Select(x => x.ErrorMessage)
            .Distinct()
            .ToArray();

        if (errors.Length == 0)
        {
            return await next();
        }

        var responseType = typeof(TResponse);

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(
                nameof(Result<object>.Failure),
                new[] { typeof(ResultErrorType), typeof(string[]) });

            return (TResponse)failureMethod!.Invoke(
                null,
                new object[] { ResultErrorType.Validation, errors })!;
        }

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(ResultErrorType.Validation, errors);
        }

        throw new ValidationException(string.Join("; ", errors));
    }
}
