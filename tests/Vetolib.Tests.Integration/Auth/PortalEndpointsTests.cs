using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Auth.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Portal endpoints (/api/v1/portal/*).
/// Wiring tests: verifies HTTP contract and auth — no business logic.
/// </summary>
public sealed class PortalEndpointsTests : IntegrationTestBase
{
    public PortalEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/portal/register ────────────────────────────────────

    [Fact]
    public async Task RegisterOwnerAccount_ValidRequest_ReturnsNon5xx()
    {
        var request = new RegisterOwnerAccountRequest(
            Email: "owner@dubai-pets.ae",
            Phone: "+971 50 123 4567",
            FullName: "Fatima Al Maktoum",
            Password: "SecureOwner1!");

        var response = await Client.PostAsJsonAsync("/api/v1/portal/register", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterOwnerAccount_DuplicateEmail_Returns4xx()
    {
        var request = new RegisterOwnerAccountRequest(
            Email: "duplicate-owner@test.ae",
            Phone: "+971 50 999 8888",
            FullName: "Ahmad Al Nahyan",
            Password: "SecureOwner1!");

        await Client.PostAsJsonAsync("/api/v1/portal/register", request, JsonOptions);
        var response = await Client.PostAsJsonAsync("/api/v1/portal/register", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.Conflict);
    }

    // ── POST /api/v1/portal/login ───────────────────────────────────────

    [Fact]
    public async Task PortalLogin_InvalidCredentials_Returns4xx()
    {
        var request = new OwnerPortalLoginRequest(
            Email: "nonexistent-owner@test.ae",
            Password: "WrongPassword1!");

        var response = await Client.PostAsJsonAsync("/api/v1/portal/login", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PortalLogin_ValidCredentials_ReturnsNon5xx()
    {
        // Register first
        var registerRequest = new RegisterOwnerAccountRequest(
            Email: "login-owner@test.ae",
            Phone: "+971 50 111 2222",
            FullName: "Sara Al Ketbi",
            Password: "OwnerLogin1!");

        await Client.PostAsJsonAsync("/api/v1/portal/register", registerRequest, JsonOptions);

        var loginRequest = new OwnerPortalLoginRequest(
            Email: "login-owner@test.ae",
            Password: "OwnerLogin1!");

        var response = await Client.PostAsJsonAsync("/api/v1/portal/login", loginRequest, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    // ── POST /api/v1/portal/invite-vet ──────────────────────────────────

    [Fact]
    public async Task InviteVet_ValidRequest_ReturnsNon5xx()
    {
        var request = new InviteVetRequest(
            VetEmail: "drvet@clinic.ae",
            OwnerName: "Khalid Al Falasi",
            PetName: "Simba",
            Message: "Please join Vetara!");

        var response = await Client.PostAsJsonAsync("/api/v1/portal/invite-vet", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    // ── POST /api/v1/portal/link-microchip ──────────────────────────────

    [Fact]
    public async Task LinkByMicrochip_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();
        var request = new LinkOwnerByMicrochipRequest("900118000123456");

        var response = await client.PostAsJsonAsync("/api/v1/portal/link-microchip", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LinkByMicrochip_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();
        var request = new LinkOwnerByMicrochipRequest("900118000123456");

        var response = await client.PostAsJsonAsync("/api/v1/portal/link-microchip", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
