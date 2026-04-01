using System.Security.Claims;
using Vetolib.Auth;
using Vetolib.Auth.Application;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class KeycloakClaimsTransformationTests
{
    private readonly KeycloakClaimsTransformation _sut = new();

    [Fact]
    public async Task TransformAsync_WhenClinicIdAlreadyPresent_DoesNotDuplicate()
    {
        var clinicId = Guid.NewGuid().ToString();
        var identity = new ClaimsIdentity(
            [new Claim("clinic_id", clinicId), new Claim("sub", "user1")],
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        var claims = result.FindAll("clinic_id").ToList();
        Assert.Single(claims);
        Assert.Equal(clinicId, claims[0].Value);
    }

    [Fact]
    public async Task TransformAsync_WhenOrganizationIdPresent_MapsToClinicId()
    {
        var orgId = Guid.NewGuid().ToString();
        var identity = new ClaimsIdentity(
            [new Claim("organization.id", orgId), new Claim("sub", "user1")],
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        var clinicClaim = result.FindFirst("clinic_id");
        Assert.NotNull(clinicClaim);
        Assert.Equal(orgId, clinicClaim.Value);
    }

    [Fact]
    public async Task TransformAsync_WhenNeitherClaimPresent_DoesNotAddClinicId()
    {
        var identity = new ClaimsIdentity(
            [new Claim("sub", "user1")],
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        Assert.Null(result.FindFirst("clinic_id"));
    }

    [Fact]
    public async Task TransformAsync_WhenNotAuthenticated_ReturnsUnchanged()
    {
        var identity = new ClaimsIdentity(); // no auth type = unauthenticated
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        Assert.Null(result.FindFirst("clinic_id"));
    }

    [Fact]
    public async Task TransformAsync_WhenBothClaimsPresent_PrefersExistingClinicId()
    {
        var clinicId = Guid.NewGuid().ToString();
        var orgId = Guid.NewGuid().ToString();
        var identity = new ClaimsIdentity(
            [
                new Claim("clinic_id", clinicId),
                new Claim("organization.id", orgId),
                new Claim("sub", "user1")
            ],
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = await _sut.TransformAsync(principal);

        var claims = result.FindAll("clinic_id").ToList();
        Assert.Single(claims);
        Assert.Equal(clinicId, claims[0].Value);
    }
}

public class PadBase64Tests
{
    [Theory]
    [InlineData("YQ", "YQ==")]       // 1 char needs 2 padding
    [InlineData("YWI", "YWI=")]      // 3 chars needs 1 padding
    [InlineData("YWJj", "YWJj")]     // 4 chars needs 0 padding
    [InlineData("a-b_c", "a+b/c")]   // base64url chars replaced, len%4==1 no padding
    public void PadBase64_PadsCorrectly(string input, string expected)
    {
        var result = AuthModuleServiceRegistrar.PadBase64(input);
        Assert.Equal(expected, result);
    }
}
