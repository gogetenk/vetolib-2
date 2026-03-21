using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vetolib.Auth.Application.Domain;

namespace Vetolib.Auth.Application.Services;

internal class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        return GenerateAccessTokenForClinic(user, user.ClinicId);
    }

    public string GenerateAccessTokenForClinic(User user, Guid clinicId)
    {
        var key = _configuration["Jwt:Key"] ?? "super-secret-key-for-vetolib-jwt-token-generation-minimum-32-chars";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("clinic_id", clinicId.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.VetLicenseNumber))
            claimsList.Add(new Claim("vetLicense", user.VetLicenseNumber));

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Vetolib",
            audience: _configuration["Jwt:Audience"] ?? "Vetolib",
            claims: claimsList.ToArray(),
            notBefore: now,
            expires: now.AddMinutes(15),
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
