using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions;

/// <summary>
/// Shared step definitions used across multiple feature files.
/// Eliminates ambiguous step binding errors.
/// </summary>
[Binding]
internal class SharedSteps
{
    private readonly ScenarioContext _ctx;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public SharedSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── Shared clinic context ───────────────────────────────────

    [Given(@"a clinic ""(.*)""")]
    public void GivenAClinic(string clinicName)
    {
        var clinicIds = GetOrCreateClinicIds();

        // The FIRST clinic registered in a scenario gets the fixed TestClinicGuid so that
        // the EF Core compiled query filter (which reads IClinicContext.ClinicId at query time)
        // matches data inserted under the "primary" test tenant.
        // SUBSEQUENT clinics get unique GUIDs so that tenant-isolation scenarios can verify
        // that data from one clinic is NOT visible to another.
        var clinicId = clinicIds.Count == 0
            ? TestClinicContext.TestClinicGuid
            : GenerateGuidFromString(clinicName);
        clinicIds[clinicName] = clinicId;

        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        // Point the current tenant context to the primary (first) clinic.
        testClinicContext.ClinicId = TestClinicContext.TestClinicGuid;

        _ctx.Set(clinicIds, "ClinicIds");
    }

    // ─── Shared authentication ───────────────────────────────────

    [Given(@"I am authenticated as (.*)")]
    public async Task GivenIAmAuthenticatedAs(string role)
    {
        await AuthenticateAsRole(role);
    }

    [Given(@"I am logged in as a VET")]
    public async Task GivenIAmLoggedInAsVet()
    {
        await AuthenticateAsRole("VET");
    }

    [Given(@"I am logged in as a RECEPTIONIST")]
    public async Task GivenIAmLoggedInAsReceptionist()
    {
        await AuthenticateAsRole("RECEPTIONIST");
    }

    [Given(@"I am logged in as an ASSISTANT")]
    public async Task GivenIAmLoggedInAsAssistant()
    {
        await AuthenticateAsRole("ASSISTANT");
    }

    [Given(@"I am logged in as an ADMIN")]
    public async Task GivenIAmLoggedInAsAdmin()
    {
        await AuthenticateAsRole("ADMIN");
    }

    // ─── Shared error assertion ──────────────────────────────────

    [Then(@"the system rejects with code ""(.*)""")]
    public void ThenTheSystemRejectsWithCode(string errorCode)
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        response.IsSuccessStatusCode.Should().BeFalse();

        switch (errorCode)
        {
            case "INSUFFICIENT_PERMISSIONS":
            case "FORBIDDEN":
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
                break;
            case "VALIDATION_ERROR":
                // Ardalis.Result serializes validation errors as a JSON array
                // with fields "identifier", "errorMessage", "errorCode", "severity".
                // There is no literal "VALIDATION_ERROR" string in the body.
                response.StatusCode.Should().BeOneOf(
                    new[] { HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity },
                    $"Expected a 400/422 for code '{errorCode}' but got {(int)response.StatusCode}");
                var validationBody = _ctx.ContainsKey("ErrorResponseBody")
                    ? _ctx.Get<string>("ErrorResponseBody")
                    : null;
                if (validationBody != null)
                    validationBody.Should().ContainAny(
                        new[] { "errorMessage", "identifier", "errors" },
                        "Expected validation error body with 'errorMessage' or 'identifier' field");
                break;
            default:
                var body = _ctx.ContainsKey("ErrorResponseBody")
                    ? _ctx.Get<string>("ErrorResponseBody")
                    : null;
                body.Should().NotBeNull();
                body.Should().Contain(errorCode);
                break;
        }
    }

    // ─── Shared error message assertion ──────────────────────────

    [Then(@"the error message is ""(.*)""")]
    public void ThenTheErrorMessageIs(string expectedMessage)
    {
        var body = _ctx.ContainsKey("ErrorResponseBody")
            ? _ctx.Get<string>("ErrorResponseBody")
            : null;
        body.Should().NotBeNull();
        body.Should().Contain(expectedMessage);
    }

    // ─── Core authentication logic ──────────────────────────────

    private async Task AuthenticateAsRole(string role)
    {
        var clinicIds = GetOrCreateClinicIds();
        var clinicName = clinicIds.Keys.FirstOrDefault() ?? "default-clinic";
        if (!clinicIds.ContainsKey(clinicName))
        {
            // Use the fixed TestClinicGuid for the default clinic to match the
            // multi-tenant query filter — avoids data invisibility issues.
            clinicIds[clinicName] = TestClinicContext.TestClinicGuid;
            _ctx.Set(clinicIds, "ClinicIds");
        }
        var clinicId = clinicIds[clinicName];

        var factory = _ctx.Get<TestWebApplicationFactory>();
        var client = _ctx.Get<HttpClient>();

        var email = $"{role.ToLowerInvariant()}@test-shared.com";
        var password = "SecurePass1";

        // Set clinic context
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        // Create user in auth DB
        using var scope = factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ADMIN" => UserRole.Admin,
            "ASSISTANT" => UserRole.Assistant,
            _ => UserRole.Receptionist
        };

        var vetLicense = userRole == UserRole.Vet ? "TEST-VET-001" : null;
        var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue($"User creation should succeed for role {role}");

        // Check if user already exists
        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        // Login to get JWT
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set(role, "CurrentRole");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Dictionary<string, Guid> GetOrCreateClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");

        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }

    internal static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
