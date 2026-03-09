using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Application.Services;

internal interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
