using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Shared Record endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth, security).
/// The public GET /api/v1/shared/{token} is anonymous and security-sensitive.
/// </summary>
public sealed class SharedRecordEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public SharedRecordEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // -- GET /api/v1/shared/{token} (anonymous, public) --

    [Fact]
    public async Task GetSharedRecord_InvalidToken_ReturnsNotFound()
    {
        // Anonymous access with a non-existent token should return 404
        var response = await Client.WithoutAuth().GetAsync("/api/v1/shared/invalid-token-abc123");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetSharedRecord_ValidFormatToken_ReturnsNon5xx()
    {
        // A well-formed but non-existent token should not cause a 500
        var fakeToken = Guid.NewGuid().ToString("N");
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/shared/{fakeToken}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.Gone);
    }

    [Fact]
    public async Task GetSharedRecord_EmptyToken_ReturnsNon5xx()
    {
        // Edge case: very short token
        var response = await Client.WithoutAuth().GetAsync("/api/v1/shared/x");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    // -- POST /api/v1/portal/animals/{id}/share --

    [Fact]
    public async Task CreateShareLink_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/share", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateShareLink_AuthenticatedWithoutOwnerClaim_ReturnsUnauthorized()
    {
        // Vet tokens don't carry owner_account_id -> endpoint returns Unauthorized
        var vetClient = CreateVetClient();

        var response = await vetClient.PostAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/share", null);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    // -- GET /api/v1/portal/shares --

    [Fact]
    public async Task ListShareLinks_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/portal/shares");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListShareLinks_AuthenticatedWithoutOwnerClaim_ReturnsUnauthorized()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync("/api/v1/portal/shares");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    // -- DELETE /api/v1/portal/shares/{id} --

    [Fact]
    public async Task RevokeShareLink_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().DeleteAsync(
            $"/api/v1/portal/shares/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RevokeShareLink_AuthenticatedWithoutOwnerClaim_ReturnsUnauthorized()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.DeleteAsync(
            $"/api/v1/portal/shares/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }
}
