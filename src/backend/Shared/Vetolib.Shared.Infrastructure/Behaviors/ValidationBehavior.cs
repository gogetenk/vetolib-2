using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Vetolib.Shared.Infrastructure.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next();

        var errors = failures
            .Select(f => new ValidationError
            {
                Identifier = f.PropertyName,
                ErrorMessage = f.ErrorMessage,
                ErrorCode = f.ErrorCode,
                Severity = f.Severity switch
                {
                    FluentValidation.Severity.Warning => ValidationSeverity.Warning,
                    FluentValidation.Severity.Info => ValidationSeverity.Info,
                    _ => ValidationSeverity.Error
                }
            })
            .ToList();

        // Handle Result (non-generic)
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Invalid(errors);

        // Handle Result<T>
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var invalidMethod = responseType.GetMethod(nameof(Result.Invalid),
                new[] { typeof(IEnumerable<ValidationError>) });
            if (invalidMethod is not null)
                return (TResponse)invalidMethod.Invoke(null, new object[] { errors })!;
        }

        // TResponse is neither Result nor Result<T> — this should never happen in this codebase.
        // Throwing here surfaces configuration mistakes at startup rather than silently skipping validation.
        throw new InvalidOperationException(
            $"ValidationBehavior<{typeof(TRequest).Name}, {typeof(TResponse).Name}>: " +
            $"TResponse must be Result or Result<T> (Ardalis.Result). " +
            $"Register handlers with the correct return type.");
    }
}
