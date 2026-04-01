using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Vetolib.Auth.Application;

/// <summary>
/// Maps Keycloak organization claims to the flat "clinic_id" claim
/// that <see cref="Vetolib.Shared.Infrastructure.ClinicContext"/> expects.
///
/// Keycloak Organizations emit "organization.id" in the access token.
/// If the token already carries a "clinic_id" (via the protocol mapper),
/// this transformation is a no-op.
/// </summary>
internal sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    private const string ClinicIdClaim = "clinic_id";
    private const string KeycloakOrgIdClaim = "organization.id";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // If clinic_id is already present (via Keycloak protocol mapper), skip.
        if (identity.HasClaim(c => c.Type == ClinicIdClaim))
            return Task.FromResult(principal);

        // Fallback: map organization.id → clinic_id
        var orgClaim = identity.FindFirst(KeycloakOrgIdClaim);
        if (orgClaim is not null)
        {
            identity.AddClaim(new Claim(ClinicIdClaim, orgClaim.Value));
        }

        return Task.FromResult(principal);
    }
}
