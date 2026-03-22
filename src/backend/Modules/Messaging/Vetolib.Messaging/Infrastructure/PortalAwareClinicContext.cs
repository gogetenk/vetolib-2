using Microsoft.AspNetCore.Http;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// IClinicContext implementation that resolves ClinicId from either:
/// 1. The JWT "clinic_id" claim (standard auth flow), or
/// 2. The portal context populated by MagicLinkEndpointFilter (portal auth flow).
///
/// This eliminates the need for IgnoreQueryFilters() in portal handlers,
/// ensuring the multi-tenant global query filter works correctly for both flows.
/// </summary>
internal class PortalAwareClinicContext : IClinicContext
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IPortalContext _portalContext;

    public PortalAwareClinicContext(IHttpContextAccessor accessor, IPortalContext portalContext)
    {
        _accessor = accessor;
        _portalContext = portalContext;
    }

    public Guid ClinicId
    {
        get
        {
            // 1. Try JWT claim first (standard authenticated requests)
            var claim = _accessor.HttpContext?.User.FindFirst("clinic_id");
            if (claim is not null && Guid.TryParse(claim.Value, out var jwtClinicId) && jwtClinicId != Guid.Empty)
                return jwtClinicId;

            // 2. Fall back to portal context (magic link portal requests)
            if (_portalContext.ClinicId != Guid.Empty)
                return _portalContext.ClinicId;

            return Guid.Empty;
        }
    }
}
