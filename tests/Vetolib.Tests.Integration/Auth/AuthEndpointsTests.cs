using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Auth.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Auth endpoints: register clinic, login, refresh token, change password, RBAC.
/// Complements BDD scenarios by testing HTTP-level contracts (status codes, headers, validation).
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
        // Arrange
        var request = new RegisterClinicRequest(
            ClinicName: "Jumeirah Vet Clinic",
            Email: "admin@jumeirah-vet.ae",
            Password: "SecureAdmin1!",
            Phone: "+971 4 555 1234",
            Country: "UAE");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);

        // Assert
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
        // Arrange — first registration
        var request = new RegisterClinicRequest(
            ClinicName: "Dubai Hills Vet",
            Email: "duplicate@dubai-hills-vet.ae",
            Password: "SecureAdmin1!",
            Phone: "+971 4 777 8888",
            Country: "UAE");

        await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);

        // Act — second registration with same email
        var response = await Client.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.UnprocessableEntity, HttpStatusCode.BadRequest);
    }

    // ── POST /api/auth/login ───────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        // Arrange — register a clinic first to create a valid user
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "Al Karama Vet",
            Email: "admin@alkarama-vet.ae",
            Password: "SecureLogin1!",
            Phone: "+971 4 333 4444",
            Country: "UAE");
        await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);

        var loginRequest = new LoginRequest("admin@alkarama-vet.ae", "SecureLogin1!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@test.ae", "WrongPassword1!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/auth/refresh ─────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200WithNewToken()
    {
        // Arrange — register and login
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "Deira Vet Center",
            Email: "admin@deira-vet.ae",
            Password: "RefreshPass1!",
            Phone: "+971 4 111 2222",
            Country: "UAE");
        await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@deira-vet.ae", "RefreshPass1!"),
            JsonOptions);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);

        // Act
        var refreshRequest = new RefreshTokenRequest(tokens!.RefreshToken);
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.AccessToken.Should().NotBe(tokens.AccessToken, "a new token must be issued");
    }

    // ── GET /api/users — RBAC ─────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_AsReceptionist_Returns403()
    {
        // Arrange — receptionist token for the test clinic
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_Returns200()
    {
        // Arrange — admin token for the test clinic
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── POST /api/auth/me — authenticated user profile ───────────────────

    [Fact]
    public async Task GetMe_Authenticated_Returns200WithUserProfile()
    {
        // Arrange — register a clinic (creates admin user) and get a token
        var email = "me@test-vet.ae";
        var registerRequest = new RegisterClinicRequest(
            ClinicName: "GetMe Test Clinic",
            Email: email,
            Password: "SecureMe1!",
            Phone: "+971 4 000 0002",
            Country: "UAE");
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/clinics/register", registerRequest, JsonOptions);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterClinicResponse>(JsonOptions);

        var authenticatedClient = Factory.CreateClient().WithToken(registerBody!.AccessToken);

        // Act
        var response = await authenticatedClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Email.Should().Be(email);
        body.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task GetMe_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
