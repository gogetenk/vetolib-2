using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vetolib.Auth.Contracts;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Generates JWT tokens for integration tests using the same key as the application.
/// Claims match exactly what JwtTokenService produces so the API authentication middleware accepts them.
/// </summary>
public static class TestJwtGenerator
{
    // Must match Jwt:Key in appsettings.Development.json
    private const string JwtKey = "super-secret-key-for-vetolib-jwt-token-generation-minimum-32-chars";
    private const string JwtIssuer = "Vetolib";
    private const string JwtAudience = "Vetolib";

    public static string GenerateToken(
        Guid userId,
        string email,
        Guid clinicId,
        UserRole role,
        string? vetLicenseNumber = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim("clinic_id", clinicId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(vetLicenseNumber))
            claimsList.Add(new Claim("vetLicense", vetLicenseNumber));

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claimsList,
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateAdminToken(Guid clinicId)
        => GenerateToken(
            userId: Guid.NewGuid(),
            email: "admin@test.ae",
            clinicId: clinicId,
            role: UserRole.Admin);

    public static string GenerateVetToken(Guid clinicId, Guid? userId = null)
        => GenerateToken(
            userId: userId ?? Guid.NewGuid(),
            email: "vet@test.ae",
            clinicId: clinicId,
            role: UserRole.Vet,
            vetLicenseNumber: "UAE-VET-TEST-001");

    public static string GenerateReceptionistToken(Guid clinicId)
        => GenerateToken(
            userId: Guid.NewGuid(),
            email: "receptionist@test.ae",
            clinicId: clinicId,
            role: UserRole.Receptionist);
}
