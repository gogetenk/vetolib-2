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
    // Fixed test key — the factory overrides Jwt:Key/Issuer/Audience with these values
    // so generated tokens always match, regardless of user-secrets on the dev machine.
    public const string JwtKey = "test-integration-jwt-key-for-tests-minimum-32-chars!!";
    public const string JwtIssuer = "Vetolib";
    public const string JwtAudience = "Vetolib";

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
