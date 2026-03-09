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
        PropertyNameCaseInsensitive = true
    };

    public SharedSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── Shared clinic context ───────────────────────────────────

    [Given(@"une clinique ""(.*)""")]
    public void GivenUneClinique(string clinicName)
    {
        var clinicIds = GetOrCreateClinicIds();
        var clinicId = GenerateGuidFromString(clinicName);
        clinicIds[clinicName] = clinicId;

        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        if (clinicIds.Count == 1)
        {
            testClinicContext.ClinicId = clinicId;
        }

        _ctx.Set(clinicIds, "ClinicIds");
    }

    // ─── Shared authentication ───────────────────────────────────

    [Given(@"je suis authentifié en tant que (.*)")]
    public async Task GivenJeSuisAuthentifie(string role)
    {
        var clinicIds = GetOrCreateClinicIds();
        var clinicName = clinicIds.Keys.First();
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
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set(role, "CurrentRole");
    }

    // ─── Shared error assertion ──────────────────────────────────

    [Then(@"le système refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        response.IsSuccessStatusCode.Should().BeFalse();

        switch (errorCode)
        {
            case "INSUFFICIENT_PERMISSIONS":
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
