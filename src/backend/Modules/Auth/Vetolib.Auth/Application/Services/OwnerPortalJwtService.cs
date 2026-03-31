using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Application.Services;

internal class OwnerPortalJwtService : IOwnerPortalJwtService
{
    private readonly IConfiguration _configuration;
    private readonly AuthSecurityOptions _securityOptions;

    public OwnerPortalJwtService(IConfiguration configuration, IOptions<AuthSecurityOptions> securityOptions)
    {
        _configuration = configuration;
        _securityOptions = securityOptions.Value;
    }

    public string GenerateOwnerPortalToken(OwnerAccount account, Guid[] linkedClinicIds)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key configuration is required.");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim("account_type", "owner_portal"),
            new Claim("owner_account_id", account.Id.ToString()),
            new Claim("linked_clinic_ids", JsonSerializer.Serialize(linkedClinicIds.Select(id => id.ToString()))),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Vetolib",
            audience: _configuration["Jwt:Audience"] ?? "Vetolib",
            claims: claimsList.ToArray(),
            notBefore: now,
            expires: now.AddMinutes(_securityOptions.TokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
