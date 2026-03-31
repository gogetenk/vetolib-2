using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Application.Services;

internal interface IOwnerPortalJwtService
{
    string GenerateOwnerPortalToken(OwnerAccount account, Guid[] linkedClinicIds);
    string GenerateRefreshToken();
}
