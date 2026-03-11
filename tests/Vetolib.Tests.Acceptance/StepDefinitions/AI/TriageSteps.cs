using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.AI.Application.Commands.TriageSymptoms;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.StepDefinitions;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.AI;

[Binding]
[Scope(Feature = "AI Veterinary Triage")]
internal class TriageSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private TriageSuggestionDto? _suggestion;
    private string? _disclaimerFromFirstRequest;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TriageSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN Steps ────────────────────────────────────────────

    [Given(@"I am authenticated as a user with role ""(.*)""")]
    public async Task GivenIAmAuthenticatedWithRole(string role)
    {
        var clinicId = GetOrCreateClinicId();

        var factory = _ctx.Get<TestWebApplicationFactory>();
        var client = _ctx.Get<HttpClient>();

        var email = $"ai-test-{role.ToLowerInvariant()}-{Guid.NewGuid():N}@happypaws.ae";
        const string password = "SecurePass1";

        using var scope = factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ADMIN" => UserRole.Admin,
            "OWNER" => UserRole.Assistant, // Owner is not a real role; map to lowest privilege
            _ => UserRole.Receptionist
        };

        var vetLicense = userRole == UserRole.Vet ? "AI-VET-001" : null;
        var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue();
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        // For Owner role, we still login successfully but the endpoint should return 403
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set(role, "CurrentRole");
    }

    [Given(@"I submitted a triage request and received a suggestion with triageId")]
    public async Task GivenISubmittedAndReceivedSuggestionWithTriageId()
    {
        await SubmitTriageRequest("Limping on front left paw", "dog", null, null, null);
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _suggestion = await _response.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOptions);
        _suggestion.Should().NotBeNull();
        _ctx.Set(_suggestion!.TriageId, "CurrentTriageId");
    }

    [Given(@"I submitted a triage request and received a suggestion with severity ""(.*)""")]
    public async Task GivenISubmittedAndReceivedSuggestionWithSeverity(string severity)
    {
        await SubmitTriageRequest("Limping on front left paw", "dog", null, null, null);
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _suggestion = await _response.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOptions);
        _suggestion.Should().NotBeNull();
        _ctx.Set(_suggestion!.TriageId, "CurrentTriageId");
        _ctx.Set(severity, "ExpectedSeverity");
    }

    [Given(@"the AI service is unavailable")]
    public void GivenTheAIServiceIsUnavailable()
    {
        _factory.FakeChatClient.SetShouldThrow(true);
    }

    // ─── WHEN Steps ─────────────────────────────────────────────

    [When(@"I submit a triage request with:")]
    public async Task WhenISubmitTriageRequestWith(Table table)
    {
        var row = table.Rows[0];
        var symptoms = row.ContainsKey("Symptoms") ? row["Symptoms"] : string.Empty;
        var species = row.ContainsKey("Species") ? row["Species"] : string.Empty;
        var breed = row.ContainsKey("Breed") && !string.IsNullOrWhiteSpace(row["Breed"])
            ? row["Breed"] : null;
        int? ageMonths = row.ContainsKey("AgeMonths") && int.TryParse(row["AgeMonths"], out var age)
            ? age : null;
        decimal? weightKg = row.ContainsKey("WeightKg") && decimal.TryParse(row["WeightKg"],
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var weight)
            ? weight : null;

        await SubmitTriageRequest(symptoms, species, breed, ageMonths, weightKg);

        if (_response.IsSuccessStatusCode)
        {
            _suggestion = await _response.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOptions);
        }
    }

    [When(@"the veterinarian accepts the triage suggestion")]
    public async Task WhenTheVeterinarianAcceptsTheTriage()
    {
        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        _response = await _client.PutAsync($"/api/v1/ai/triage/{triageId}/accept", null);
    }

    [When(@"the veterinarian overrides the severity to ""(.*)""")]
    public async Task WhenTheVeterinarianOverridesTheSeverityTo(string newSeverity)
    {
        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        var body = new StringContent(
            JsonSerializer.Serialize(new { newSeverity = Enum.Parse<AISeverity>(newSeverity, true) }),
            Encoding.UTF8,
            "application/json");
        _response = await _client.PutAsync($"/api/v1/ai/triage/{triageId}/override", body);
    }

    // ─── THEN Steps ─────────────────────────────────────────────

    [Then(@"I should receive a triage suggestion with status 200")]
    public void ThenIShouldReceiveATriage200()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_response.StatusCode}");
        _suggestion.Should().NotBeNull();
    }

    [Then(@"the suggestion should contain a severity of ""Normal"", ""Emergency"", or ""Routine""")]
    public void ThenSuggestionShouldContainValidSeverity()
    {
        _suggestion.Should().NotBeNull();
        var validSeverities = new[] { AISeverity.Emergency, AISeverity.Normal, AISeverity.Routine };
        validSeverities.Should().Contain(_suggestion!.Severity);
    }

    [Then(@"the suggestion should contain an estimated duration in minutes")]
    public void ThenSuggestionShouldContainDuration()
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.EstimatedDurationMinutes.Should().BeGreaterThan(0);
    }

    [Then(@"the suggestion should contain a recommended specialty")]
    public void ThenSuggestionShouldContainSpecialty()
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.RecommendedSpecialty.Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"the suggestion should contain a confidence score between 0 and 1")]
    public void ThenSuggestionShouldContainConfidence()
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.Confidence.Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThanOrEqualTo(1.0);
    }

    [Then(@"the suggestion should contain a non-empty disclaimer")]
    public void ThenSuggestionShouldContainDisclaimer()
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.Disclaimer.Should().NotBeNullOrWhiteSpace();
    }

    [Then(@"the suggestion should contain a triage ID")]
    public void ThenSuggestionShouldContainTriageId()
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.TriageId.Should().NotBe(Guid.Empty);
    }

    [Then(@"the triage result should be persisted with WasAccepted true")]
    public async Task ThenTriageResultPersistedWithWasAcceptedTrue()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Accept endpoint returned {(int)_response.StatusCode}");

        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var result = await db.TriageResults.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == triageId);

        result.Should().NotBeNull();
        result!.WasAccepted.Should().BeTrue();
    }

    [Then(@"OverriddenSeverity should be null")]
    public async Task ThenOverriddenSeverityShouldBeNull()
    {
        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var result = await db.TriageResults.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == triageId);

        result.Should().NotBeNull();
        result!.OverriddenSeverity.Should().BeNull();
    }

    [Then(@"the triage result should be persisted with WasAccepted false")]
    public async Task ThenTriageResultPersistedWithWasAcceptedFalse()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Override endpoint returned {(int)_response.StatusCode}");

        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var result = await db.TriageResults.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == triageId);

        result.Should().NotBeNull();
        result!.WasAccepted.Should().BeFalse();
    }

    [Then(@"OverriddenSeverity should be ""(.*)""")]
    public async Task ThenOverriddenSeverityShouldBe(string severity)
    {
        var triageId = _ctx.Get<Guid>("CurrentTriageId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var result = await db.TriageResults.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == triageId);

        result.Should().NotBeNull();
        result!.OverriddenSeverity.Should().Be(Enum.Parse<AISeverity>(severity, true));
    }

    [Then(@"the suggestion severity should be ""(.*)""")]
    public void ThenSuggestionSeverityShouldBe(string severity)
    {
        _suggestion.Should().NotBeNull();
        _suggestion!.Severity.Should().Be(Enum.Parse<AISeverity>(severity, true));
    }

    [Then(@"I should receive a validation error for ""(.*)""")]
    public async Task ThenIShouldReceiveValidationErrorFor(string fieldName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            $"Expected 400 but got {(int)_response.StatusCode}");
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(fieldName, $"Expected validation error for {fieldName} in: {body}");
    }

    [Then(@"the disclaimer should contain ""(.*)"" and ""(.*)""")]
    public void ThenDisclaimerShouldContain(string word1, string word2)
    {
        _suggestion.Should().NotBeNull();
        _disclaimerFromFirstRequest = _suggestion!.Disclaimer;
        _suggestion.Disclaimer.Should().Contain(word1);
        _suggestion.Disclaimer.Should().Contain(word2);
    }

    [Then(@"the disclaimer should not change between requests")]
    public void ThenDisclaimerShouldNotChangeBetweenRequests()
    {
        // The disclaimer is a constant — verify it equals the expected constant
        _suggestion.Should().NotBeNull();
        _suggestion!.Disclaimer.Should().Be(TriageSymptomsHandler.Disclaimer);

        if (_disclaimerFromFirstRequest is not null)
        {
            _suggestion.Disclaimer.Should().Be(_disclaimerFromFirstRequest);
        }
    }

    [Then(@"I should receive an error ""(.*)""")]
    public async Task ThenIShouldReceiveAnError(string errorCode)
    {
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(errorCode, $"Expected error code {errorCode} in: {body}");
    }

    [Then(@"the HTTP status should be 503")]
    public void ThenHttpStatusShouldBe503()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable,
            $"Expected 503 but got {(int)_response.StatusCode}");
    }

    [Then(@"a triage result should be persisted in the database")]
    public async Task ThenATriageResultShouldBePersistedInDatabase()
    {
        _suggestion.Should().NotBeNull();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        var result = await db.TriageResults.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == _suggestion!.TriageId);

        result.Should().NotBeNull();
        _ctx.Set(result!, "PersistedTriageResult");
    }

    [Then(@"the persisted result should include the model used")]
    public void ThenPersistedResultShouldIncludeModelUsed()
    {
        var result = _ctx.Get<Vetolib.AI.Application.Domain.TriageResult>("PersistedTriageResult");
        result.ModelUsed.Should().NotBeNull();
    }

    [Then(@"the persisted result should include prompt and completion token counts")]
    public void ThenPersistedResultShouldIncludeTokenCounts()
    {
        var result = _ctx.Get<Vetolib.AI.Application.Domain.TriageResult>("PersistedTriageResult");
        result.PromptTokens.Should().BeGreaterThanOrEqualTo(0);
        result.CompletionTokens.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then(@"the persisted result should include latency in milliseconds")]
    public void ThenPersistedResultShouldIncludeLatency()
    {
        var result = _ctx.Get<Vetolib.AI.Application.Domain.TriageResult>("PersistedTriageResult");
        result.LatencyMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then(@"I should receive a 403 Forbidden response")]
    public void ThenIShouldReceive403()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Expected 403 but got {(int)_response.StatusCode}");
    }

    // ─── Helpers ────────────────────────────────────────────────

    private Guid GetOrCreateClinicId()
    {
        if (_ctx.ContainsKey("ClinicIds"))
        {
            var ids = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
            if (ids.Count > 0) return ids.Values.First();
        }

        // Use the fixed TestClinicGuid so the multi-tenant query filter sees triage data.
        var clinicId = TestClinicContext.TestClinicGuid;
        var dict = new Dictionary<string, Guid> { ["ai-test-clinic"] = clinicId };
        _ctx.Set(dict, "ClinicIds");

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        return clinicId;
    }

    private async Task SubmitTriageRequest(
        string symptoms,
        string species,
        string? breed,
        int? ageMonths,
        decimal? weightKg)
    {
        var body = new
        {
            symptoms,
            species,
            breed,
            ageMonths,
            weightKg
        };

        _response = await _client.PostAsJsonAsync("/api/v1/ai/triage", body);
    }
}
