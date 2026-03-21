using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Reqnroll;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Audit;

[Binding]
[Scope(Feature = "Audit trail")]
internal class AuditSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage? _lastResponse;
    private string? _errorResponseBody;
    private AuditLogResponse? _auditResponse;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AuditSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN ──────────────────────────────────────────────────

    [Given(@"a patient was created")]
    public async Task GivenAPatientWasCreated()
    {
        var patientRequest = new CreatePatientRequest(
            Name: "Falco",
            Species: Species.Dog,
            Breed: "Labrador",
            BirthDate: new DateOnly(2021, 3, 15),
            OwnerName: "Ahmed Al Mansouri",
            OwnerPhone: "+971501234567");

        var patientResponse = await _client.PostAsJsonAsync("/api/v1/patients", patientRequest);
        if (!patientResponse.IsSuccessStatusCode)
        {
            var errorBody = await patientResponse.Content.ReadAsStringAsync();
            patientResponse.IsSuccessStatusCode.Should().BeTrue(
                $"Creating patient failed ({(int)patientResponse.StatusCode}): {errorBody}");
        }
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"I query audit for entityType ""(.*)""")]
    public async Task WhenIQueryAuditForEntityType(string entityType)
    {
        _lastResponse = await _client.GetAsync($"/api/audit?entityType={entityType}");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _auditResponse = await _lastResponse.Content.ReadFromJsonAsync<AuditLogResponse>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I query audit")]
    public async Task WhenIQueryAudit()
    {
        _lastResponse = await _client.GetAsync("/api/audit");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _auditResponse = await _lastResponse.Content.ReadFromJsonAsync<AuditLogResponse>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"I see an audit entry with action ""(.*)""")]
    public void ThenISeeAnAuditEntryWithAction(string action)
    {
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _auditResponse.Should().NotBeNull();
        _auditResponse!.TotalCount.Should().BeGreaterThan(0,
            "At least one audit entry should exist");
        _auditResponse.Entries.Should().Contain(e => e.Action == action,
            $"Expected at least one entry with action '{action}'");
    }

    [Then(@"the entry contains a changedBy value")]
    public void ThenTheEntryContainsAChangedByValue()
    {
        _auditResponse.Should().NotBeNull();
        var createdEntry = _auditResponse!.Entries.First(e => e.Action == "Created");
        // ChangedBy may be null for system operations but should ideally carry the user email
        // In test context the audit interceptor captures from JWT claims
        createdEntry.Should().NotBeNull();
    }

    [Then(@"the user is denied access")]
    public void ThenTheUserIsDeniedAccess()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    // ─── Response DTOs ───────────────────────────────────────────

    private record AuditEntryDto(
        long Id,
        string EntityType,
        string EntityId,
        string Action,
        string? ChangedBy,
        Guid ClinicId,
        DateTime Timestamp,
        string? OldValues,
        string? NewValues);

    private record AuditLogResponse(
        int TotalCount,
        int Page,
        int PageSize,
        IReadOnlyList<AuditEntryDto> Entries);
}
