using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// Validates a magic link token from query string (?token=...) or X-Portal-Token header.
/// Populates OwnerId and ClinicId in HttpContext.Items for downstream handlers via IPortalContext.
/// </summary>
internal class MagicLinkEndpointFilter : IEndpointFilter
{
    private readonly MessagingDbContext _context;

    public MagicLinkEndpointFilter(MessagingDbContext context)
    {
        _context = context;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var tokenValue = httpContext.Request.Query["token"].FirstOrDefault()
            ?? httpContext.Request.Headers["X-Portal-Token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return Results.Unauthorized();
        }

        // Look up token without tenant filter (token lookup is cross-tenant by design)
        var token = await _context.OwnerPortalTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Token == tokenValue, httpContext.RequestAborted);

        if (token is null)
            return Results.Unauthorized();

        if (!token.IsValid())
        {
            return Results.Json(
                new { error = "This link has expired. Please contact your clinic to receive a new one." },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        PortalContext.Populate(httpContext, token.OwnerId, token.ClinicId);

        return await next(context);
    }
}
