using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

/// <summary>
/// Metadata attribute that marks an endpoint as requiring a subscription limit check.
/// Use with <see cref="SubscriptionCheckFilter"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CheckLimitAttribute : Attribute
{
    public LimitType LimitType { get; }

    public CheckLimitAttribute(LimitType limitType)
    {
        LimitType = limitType;
    }
}

/// <summary>
/// Endpoint filter that enforces subscription plan limits.
/// Reads <see cref="CheckLimitAttribute"/> from endpoint metadata and calls <see cref="ISubscriptionChecker"/>.
/// Returns 403 Forbidden if the clinic's plan does not allow the action.
/// </summary>
internal class SubscriptionCheckFilter : IEndpointFilter
{
    private readonly ISubscriptionChecker _checker;

    public SubscriptionCheckFilter(ISubscriptionChecker checker)
    {
        _checker = checker;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var checkLimitAttr = endpoint?.Metadata.GetMetadata<CheckLimitAttribute>();

        if (checkLimitAttr is null)
            return await next(context);

        var clinicIdClaim = context.HttpContext.User.FindFirst("clinicId")?.Value;
        if (clinicIdClaim is null || !Guid.TryParse(clinicIdClaim, out var clinicId))
            return Results.Forbid();

        var result = await _checker.CheckLimitAsync(clinicId, checkLimitAttr.LimitType);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Plan limit exceeded. Please upgrade.";
            return Results.Json(
                new { error = errorMessage },
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
