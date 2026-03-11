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

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "Change password")]
internal class ChangePasswordSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private string _currentUserEmail = string.Empty;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ChangePasswordSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN Steps ─────────────────────────────────────────────

    [Given(@"I am authenticated as VET with password ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsVetWithPassword(string password)
    {
        var clinicId = GetClinicId();

        // Create user directly in DB
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var email = $"vet-cp-{Guid.NewGuid():N}@happypaws.ae";
        _currentUserEmail = email;

        var userResult = User.Create(clinicId, email, password, UserRole.Vet, "UAE-VET-CP-001");
        userResult.IsSuccess.Should().BeTrue("User creation should succeed");

        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        // Login to get JWT
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I change my password from ""(.*)"" to ""(.*)""")]
    public async Task WhenIChangeMyPasswordFromTo(string currentPassword, string newPassword)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/auth/change-password",
            new ChangePasswordRequest(currentPassword, newPassword));
    }

    [When(@"I send a change password request without authentication")]
    public async Task WhenISendAChangePasswordRequestWithoutAuthentication()
    {
        // Ensure no auth header
        _client.DefaultRequestHeaders.Authorization = null;
        _response = await _client.PostAsJsonAsync("/api/v1/auth/change-password",
            new ChangePasswordRequest("SomePass@1234!", "NewPass@1234!"));
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int statusCode)
    {
        ((int)_response.StatusCode).Should().Be(statusCode,
            $"Expected HTTP {statusCode} but got {(int)_response.StatusCode}");
    }

    [Then(@"I can log in with the new password ""(.*)""")]
    public async Task ThenICanLogInWithTheNewPassword(string newPassword)
    {
        // Clear auth header to test a fresh login
        _client.DefaultRequestHeaders.Authorization = null;

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(_currentUserEmail, newPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login with new password should succeed for {_currentUserEmail}");
    }

    [Then(@"the error code is ""(.*)""")]
    public async Task ThenTheErrorCodeIs(string errorCode)
    {
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(errorCode,
            $"Response body should contain error code '{errorCode}'. Body: {body}");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Guid GetClinicId()
    {
        if (_ctx.ContainsKey("ClinicIds"))
        {
            var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
            if (clinicIds.Count > 0)
                return clinicIds.Values.First();
        }

        // Fallback: use the fixed TestClinicGuid for multi-tenant filter compatibility.
        return TestClinicContext.TestClinicGuid;
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
