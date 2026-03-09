using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Behaviors;

internal class ValidationBehavior<TRequest, TResponse>
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
            .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
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

        return await next();
    }
}
