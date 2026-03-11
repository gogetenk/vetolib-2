using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Auth.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Auth endpoints.
/// Auth routes: /api/v1/auth/* (login, refresh, me, logout)
/// User routes: /api/v1/users/* (list, invite, change role, deactivate)
/// Clinic routes: /api/v1/clinics/* (register)
/// </summary>
public sealed class AuthEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AuthEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/clinics/register ──────────────────────────────────────

    [Fact]
    public async Task RegisterClinic_ValidRequest_Returns201WithTokens()
    {
        var request = new RegisterClinicRequest(
            ClinicName: "Jumeirah Vet Clinic",
            Email: "admin@jumeirah-vet.ae",
            Password: "SecureAdmin1!",
            Phone: "+971 4 555 1234",
            Country: "UAE");

        var response = await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterClinicResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.ClinicId.Should().NotBeEmpty();
        body.User.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task RegisterClinic_DuplicateEmail_Returns422()
    {
        var request = new RegisterClinicRequest(
            ClinicName: "Dubai Hills Vet",
            Email: "duplicate@dubai-hills-vet.ae",
            Password: "SecureAdmin1!",
            Phone: "+971 4 777 8888",
            Country: "UAE");

        await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);
        var response = await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.UnprocessableEntity, HttpStatusCode.BadRequest);
    }

    // ── POST /api/v1/auth/login ───────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "Al Karama Vet",
            Email: "admin@alkarama-vet.ae",
            Password: "SecureLogin1!",
            Phone: "+971 4 333 4444",
            Country: "UAE");
        await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);

        var loginRequest = new LoginRequest("admin@alkarama-vet.ae", "SecureLogin1!");
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns4xx()
    {
        var loginRequest = new LoginRequest("nonexistent@test.ae", "WrongPassword1!");
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest, JsonOptions);

        // LoginHandler returns Result.Invalid for bad credentials → 422
        // or Result.Unauthorized → 401
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.UnprocessableEntity);
    }

    // ── POST /api/v1/auth/refresh ─────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200WithNewToken()
    {
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "Deira Vet Center",
            Email: "admin@deira-vet.ae",
            Password: "RefreshPass1!",
            Phone: "+971 4 111 2222",
            Country: "UAE");
        await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("admin@deira-vet.ae", "RefreshPass1!"),
            JsonOptions);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);

        var refreshRequest = new RefreshTokenRequest(tokens!.RefreshToken);
        var response = await Client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.AccessToken.Should().NotBe(tokens.AccessToken, "a new token must be issued");
    }

    // ── GET /api/v1/users — RBAC ─────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var response = await receptionistClient.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_Returns200()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/v1/auth/me — authenticated user profile ───────────────────

    [Fact]
    public async Task GetMe_Authenticated_Returns200WithUserProfile()
    {
        var email = "me@test-vet.ae";
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "GetMe Test Clinic",
            Email: email,
            Password: "SecureMe1!",
            Phone: "+971 4 000 0002",
            Country: "UAE");
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterClinicResponse>(JsonOptions);

        // The access token from registration contains the correct clinic_id claim.
        // The real ClinicContext reads clinic_id from the JWT, so no singleton manipulation needed.
        var authenticatedClient = Factory.CreateClient().WithToken(registerBody!.AccessToken);
        var response = await authenticatedClient.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task GetMe_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
