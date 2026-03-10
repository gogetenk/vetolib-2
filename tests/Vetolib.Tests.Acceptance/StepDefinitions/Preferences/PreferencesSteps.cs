using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Preferences;

[Binding]
internal class PreferencesSteps
{
    private readonly ScenarioContext _ctx;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public PreferencesSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    // ─── Shared setup steps (English variants used in Preferences feature) ───────

    [Given(@"a clinic ""(.*)""")]
    public void GivenAClinic(string clinicName)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        var clinicId = new Guid("11111111-1111-1111-1111-111111111111");
        testClinicContext.ClinicId = clinicId;
        _ctx.Set(clinicName, "ClinicName");
    }

    // ─── Shared auth step for Preferences tests ───────────────────────────────

    [Given(@"I am authenticated as (.*)")]
    public async Task GivenIAmAuthenticated(string role)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var client = _ctx.Get<HttpClient>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();

        var clinicId = testClinicContext.ClinicId;
        if (clinicId == Guid.Empty)
        {
            clinicId = new Guid("11111111-1111-1111-1111-111111111111");
            testClinicContext.ClinicId = clinicId;
        }

        var email = $"pref-{role.ToLowerInvariant()}-{clinicId:N}@test.com";
        var password = "SecurePass1";

        using var scope = factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ADMIN" => UserRole.Admin,
            _ => UserRole.Receptionist
        };

        var vetLicense = userRole == UserRole.Vet ? "PREF-VET-001" : null;
        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        Guid userId;
        if (existing is null)
        {
            var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
            userResult.IsSuccess.Should().BeTrue();
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
            userId = userResult.Value.Id;
        }
        else
        {
            userId = existing.Id;
        }

        _ctx.Set(userId, "CurrentUserId");
        _ctx.Set(role, "CurrentRole");

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = email, Password = password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authToken = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = authToken.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!);
    }

    [Given(@"a clinic default ""(.*)"" is set to ""(.*)""")]
    public async Task GivenAClinicDefaultIsSetTo(string key, string value)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        var clinicId = testClinicContext.ClinicId;

        if (!Enum.TryParse<PreferenceKey>(key, out var preferenceKey))
            throw new ArgumentException($"Unknown PreferenceKey: {key}");

        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var createResult = ClinicPreferenceDefault.Create(clinicId, preferenceKey, value);
        createResult.IsSuccess.Should().BeTrue();
        preferencesDb.ClinicPreferenceDefaults.Add(createResult.Value);
        await preferencesDb.SaveChangesAsync();
    }

    [Given(@"the user has opted in to analytics preferences")]
    public async Task GivenTheUserHasOptedInToAnalytics()
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        var clinicId = testClinicContext.ClinicId;
        var userId = _ctx.Get<Guid>("CurrentUserId");

        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var analyticsKeys = new[] { PreferenceKey.AnalyticsPosthog, PreferenceKey.AnalyticsUsageData };
        foreach (var prefKey in analyticsKeys)
        {
            var createResult = UserPreference.Create(clinicId, userId, prefKey, "true");
            if (createResult.IsSuccess)
                preferencesDb.UserPreferences.Add(createResult.Value);
        }

        await preferencesDb.SaveChangesAsync();
    }

    [Given(@"several preference changes have been recorded")]
    public async Task GivenSeveralPreferenceChangesHaveBeenRecorded()
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        var clinicId = testClinicContext.ClinicId;
        var userId = _ctx.Get<Guid>("CurrentUserId");

        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var audit1 = ConsentAuditEntry.Create(clinicId, userId, PreferenceCategory.Notifications,
            PreferenceKey.NotificationEmail, "true", "false", PreferenceSource.User);
        var audit2 = ConsentAuditEntry.Create(clinicId, userId, PreferenceCategory.Analytics,
            PreferenceKey.AnalyticsPosthog, null, "false", PreferenceSource.User);

        if (audit1.IsSuccess) preferencesDb.ConsentAuditEntries.Add(audit1.Value);
        if (audit2.IsSuccess) preferencesDb.ConsentAuditEntries.Add(audit2.Value);

        await preferencesDb.SaveChangesAsync();
    }

    // ─── When steps ───────────────────────────────────────────────────────────

    [When(@"I retrieve my preferences")]
    public async Task WhenIRetrieveMyPreferences()
    {
        var client = _ctx.Get<HttpClient>();
        var response = await client.GetAsync("/api/preferences");
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _ctx.Set(content, "PreferencesContent");
        }
    }

    [When(@"I set preference ""(.*)"" to ""(.*)""")]
    public async Task WhenISetPreference(string key, string value)
    {
        var client = _ctx.Get<HttpClient>();
        var response = await client.PutAsJsonAsync($"/api/preferences/{key}",
            new { Value = value });
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");
    }

    [When(@"I revoke consent for category ""(.*)""")]
    public async Task WhenIRevokeConsentForCategory(string category)
    {
        if (!Enum.TryParse<PreferenceCategory>(category, out var categoryEnum))
            throw new ArgumentException($"Unknown PreferenceCategory: {category}");

        var client = _ctx.Get<HttpClient>();
        var response = await client.PostAsJsonAsync("/api/preferences/consent/revoke",
            new { Category = (int)categoryEnum });
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");
    }

    [When(@"the admin sets clinic default ""(.*)"" to ""(.*)""")]
    public async Task WhenTheAdminSetsClinicDefault(string key, string value)
    {
        if (!Enum.TryParse<PreferenceKey>(key, out var keyEnum))
            throw new ArgumentException($"Unknown PreferenceKey: {key}");

        var client = _ctx.Get<HttpClient>();
        var response = await client.PutAsJsonAsync("/api/clinics/preferences",
            new { Defaults = new[] { new { Key = (int)keyEnum, Value = value } } });
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");
    }

    [When(@"the user tries to update clinic defaults with ""(.*)"" set to ""(.*)""")]
    public async Task WhenTheUserTriesToUpdateClinicDefaults(string key, string value)
    {
        if (!Enum.TryParse<PreferenceKey>(key, out var keyEnum))
            throw new ArgumentException($"Unknown PreferenceKey: {key}");

        var client = _ctx.Get<HttpClient>();
        var response = await client.PutAsJsonAsync("/api/clinics/preferences",
            new { Defaults = new[] { new { Key = (int)keyEnum, Value = value } } });
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");
    }

    [When(@"I retrieve the consent audit trail")]
    public async Task WhenIRetrieveTheConsentAuditTrail()
    {
        var client = _ctx.Get<HttpClient>();
        var response = await client.GetAsync("/api/preferences/audit");
        _ctx.Set(response, "LastResponse");
        _ctx.Set(response.StatusCode, "LastStatusCode");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _ctx.Set(content, "AuditContent");
        }
    }

    // ─── Then steps ───────────────────────────────────────────────────────────

    [Then(@"the preference ""(.*)"" has value ""(.*)"" with source ""(.*)""")]
    public async Task ThenThePreferenceHasValueWithSource(string key, string value, string source)
    {
        // If we have PreferencesContent, use it; otherwise fetch
        string? content = _ctx.ContainsKey("PreferencesContent")
            ? _ctx.Get<string>("PreferencesContent")
            : null;

        if (content is null)
        {
            var client = _ctx.Get<HttpClient>();
            var response = await client.GetAsync("/api/preferences");
            response.IsSuccessStatusCode.Should().BeTrue();
            content = await response.Content.ReadAsStringAsync();
        }

        var categories = JsonSerializer.Deserialize<List<JsonElement>>(content, JsonOptions)!;
        var allPrefs = categories
            .SelectMany(c => c.GetProperty("preferences").EnumerateArray())
            .ToList();

        // PreferenceKey enum may be serialized as integer or string depending on config
        var pref = allPrefs.FirstOrDefault(p =>
        {
            var keyProp = p.GetProperty("key");
            if (keyProp.ValueKind == JsonValueKind.String)
                return keyProp.GetString()!.Equals(key, StringComparison.OrdinalIgnoreCase);
            if (keyProp.ValueKind == JsonValueKind.Number && Enum.TryParse<PreferenceKey>(key, out var k))
                return keyProp.GetInt32() == (int)k;
            return false;
        });

        pref.ValueKind.Should().NotBe(JsonValueKind.Undefined, $"Preference '{key}' should exist");
        pref.GetProperty("value").GetString().Should().Be(value,
            $"Preference '{key}' should have value '{value}'");

        // Source may be string or int
        var sourceProp = pref.GetProperty("source");
        string actualSource;
        if (sourceProp.ValueKind == JsonValueKind.String)
            actualSource = sourceProp.GetString()!;
        else
            actualSource = ((PreferenceSource)sourceProp.GetInt32()).ToString();

        actualSource.Should().Be(source, $"Preference '{key}' should have source '{source}'");
    }

    [Then(@"the response is successful")]
    public void ThenTheResponseIsSuccessful()
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {response.StatusCode}");
    }

    [Then(@"the response status is (\d+)")]
    public async Task ThenTheResponseStatusIs(int statusCode)
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        var body = await response.Content.ReadAsStringAsync();
        ((int)response.StatusCode).Should().Be(statusCode, $"Response body: {body}");
    }

    [Then(@"a consent audit entry exists for preference ""(.*)""")]
    public async Task ThenAConsentAuditEntryExistsForPreference(string key)
    {
        if (!Enum.TryParse<PreferenceKey>(key, out var preferenceKey))
            throw new ArgumentException($"Unknown PreferenceKey: {key}");

        var factory = _ctx.Get<TestWebApplicationFactory>();
        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var exists = await preferencesDb.ConsentAuditEntries
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Key == preferenceKey);

        exists.Should().BeTrue($"Expected a consent audit entry for preference '{key}'");
    }

    [Then(@"the preference ""(.*)"" remains ""(.*)""")]
    public async Task ThenThePreferenceRemains(string key, string expectedValue)
    {
        // Fetch current preferences to verify the value hasn't changed
        var client = _ctx.Get<HttpClient>();
        var response = await client.GetAsync("/api/preferences");
        response.IsSuccessStatusCode.Should().BeTrue();

        var content = await response.Content.ReadAsStringAsync();
        var categories = JsonSerializer.Deserialize<List<JsonElement>>(content, JsonOptions)!;
        var allPrefs = categories
            .SelectMany(c => c.GetProperty("preferences").EnumerateArray())
            .ToList();

        var pref = allPrefs.FirstOrDefault(p =>
        {
            var keyProp = p.GetProperty("key");
            if (keyProp.ValueKind == JsonValueKind.String)
                return keyProp.GetString()!.Equals(key, StringComparison.OrdinalIgnoreCase);
            if (keyProp.ValueKind == JsonValueKind.Number && Enum.TryParse<PreferenceKey>(key, out var k))
                return keyProp.GetInt32() == (int)k;
            return false;
        });

        pref.ValueKind.Should().NotBe(JsonValueKind.Undefined, $"Preference '{key}' should exist");
        pref.GetProperty("value").GetString().Should().Be(expectedValue,
            $"Preference '{key}' should still have value '{expectedValue}'");
    }

    [Then(@"all analytics preferences are set to ""(.*)""")]
    public async Task ThenAllAnalyticsPreferencesAreSetTo(string expectedValue)
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var testClinicContext = factory.Services.GetRequiredService<TestClinicContext>();
        var userId = _ctx.Get<Guid>("CurrentUserId");

        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var analyticsPrefs = await preferencesDb.UserPreferences
            .IgnoreQueryFilters()
            .Where(p => p.UserId == userId && p.Category == PreferenceCategory.Analytics)
            .ToListAsync();

        analyticsPrefs.Should().NotBeEmpty("Expected analytics preferences to be set");
        analyticsPrefs.Should().AllSatisfy(p =>
            p.Value.Should().Be(expectedValue,
                $"Preference '{p.Key}' should have value '{expectedValue}'"));
    }

    [Then(@"consent audit entries exist for the revoked preferences")]
    public async Task ThenConsentAuditEntriesExistForTheRevokedPreferences()
    {
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var userId = _ctx.Get<Guid>("CurrentUserId");

        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var auditCount = await preferencesDb.ConsentAuditEntries
            .IgnoreQueryFilters()
            .CountAsync(e => e.UserId == userId && e.Category == PreferenceCategory.Analytics);

        auditCount.Should().BeGreaterThan(0, "Expected consent audit entries for revoked analytics preferences");
    }

    [Then(@"the clinic default ""(.*)"" is ""(.*)""")]
    public async Task ThenTheClinicDefaultIs(string key, string expectedValue)
    {
        if (!Enum.TryParse<PreferenceKey>(key, out var preferenceKey))
            throw new ArgumentException($"Unknown PreferenceKey: {key}");

        var factory = _ctx.Get<TestWebApplicationFactory>();
        using var scope = factory.Services.CreateScope();
        var preferencesDb = scope.ServiceProvider.GetRequiredService<PreferencesDbContext>();

        var clinicDefault = await preferencesDb.ClinicPreferenceDefaults
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Key == preferenceKey);

        clinicDefault.Should().NotBeNull($"Expected clinic default to exist for key '{key}'");
        clinicDefault!.Value.Should().Be(expectedValue);
    }

    [Then(@"the audit entries are returned")]
    public void ThenTheAuditEntriesAreReturned()
    {
        var response = _ctx.Get<HttpResponseMessage>("LastResponse");
        response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {response.StatusCode}");

        var content = _ctx.Get<string>("AuditContent");
        content.Should().NotBeNullOrEmpty();

        var result = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }
}
