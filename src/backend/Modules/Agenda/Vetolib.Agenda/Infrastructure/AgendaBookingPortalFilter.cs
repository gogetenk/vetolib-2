using Microsoft.AspNetCore.Http;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Agenda.Infrastructure;

/// <summary>
/// Validates the magic link token for the owner booking portal endpoints.
/// Reads the token from query string (?token=...) or X-Portal-Token header.
/// Populates AgendaPortalContext with OwnerId and ClinicId on success.
/// Delegates token validation to IOwnerPortalTokenValidator (Messaging.Contracts contract).
/// </summary>
internal class AgendaBookingPortalFilter : IEndpointFilter
{
    private readonly IOwnerPortalTokenValidator _validator;

    public AgendaBookingPortalFilter(IOwnerPortalTokenValidator validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var tokenValue = httpContext.Request.Query["token"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Portal-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenValue))
            return Results.Unauthorized();

        var result = await _validator.ValidateAsync(tokenValue, httpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return Results.Json(
                new { error = "This link has expired. Please contact your clinic to receive a new one." },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        AgendaPortalContext.Populate(httpContext, result.Value.OwnerId, result.Value.ClinicId);

        return await next(context);
    }
}
