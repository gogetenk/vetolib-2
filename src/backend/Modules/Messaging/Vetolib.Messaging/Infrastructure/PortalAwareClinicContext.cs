using Microsoft.AspNetCore.Http;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// An IClinicContext implementation that is aware of both regular JWT-authenticated
/// requests and portal (magic link) authenticated requests.
///
/// Priority:
///  1. HttpContext.Items["portal_clinic_id"] — set by MagicLinkEndpointFilter for portal requests.
///  2. JWT claim "clinic_id" — set by standard auth middleware for staff/vet requests.
///
/// This allows MessagingDbContext's global query filter to work correctly for portal
/// endpoints without requiring IgnoreQueryFilters() in portal handlers.
/// </summary>
internal class PortalAwareClinicContext : IClinicContext
{
    private const string PortalClinicIdKey = "portal_clinic_id";

    private readonly IHttpContextAccessor _accessor;

    public PortalAwareClinicContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid ClinicId
    {
        get
        {
            var httpContext = _accessor.HttpContext;
            if (httpContext is null)
                return Guid.Empty;

            // 1. Portal auth path: MagicLinkEndpointFilter sets portal_clinic_id in Items
            if (httpContext.Items.TryGetValue(PortalClinicIdKey, out var portalValue)
                && portalValue is Guid portalClinicId
                && portalClinicId != Guid.Empty)
            {
                return portalClinicId;
            }

            // 2. Standard JWT auth path
            var claim = httpContext.User.FindFirst("clinic_id");
            if (claim is not null && Guid.TryParse(claim.Value, out var jwtClinicId))
                return jwtClinicId;

            return Guid.Empty;
        }
    }
}
