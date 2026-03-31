using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Reqnroll;
using Vetolib.Breeding.Contracts;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Breeding;

[Binding]
[Scope(Feature = "Heat cycle tracking")]
internal class HeatCycleSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private HeatCycleDto? _lastHeatCycle;
    private List<HeatCycleDto>? _heatCycleList;
    private HeatPredictionDto? _prediction;

    private static readonly JsonSerializerOptions JsonOptions = BreedingSharedSteps.JsonOptions;

    public HeatCycleSteps(ScenarioContext ctx)
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

    [Given(@"the following heat cycles for ""(.*)"":")]
    public async Task GivenTheFollowingHeatCyclesFor(string patientName, DataTable table)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        foreach (var row in table.Rows)
        {
            var request = new RecordHeatCycleRequest(
                DateOnly.Parse(row["StartDate"]),
                DateOnly.Parse(row["EndDate"]));
            var response = await _client.PostAsJsonAsync(
                $"/api/v1/patients/{patientId}/heat-cycles", request);
            response.StatusCode.Should().BeOneOf(
                new[] { HttpStatusCode.OK, HttpStatusCode.Created },
                $"Failed to create heat cycle starting {row["StartDate"]}");
        }
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I record a heat cycle for ""(.*)"" starting on (.*) ending on (.*)")]
    public async Task WhenIRecordAHeatCycleStartingOnEndingOn(
        string patientName, string startDate, string endDate)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new RecordHeatCycleRequest(
            DateOnly.Parse(startDate),
            DateOnly.Parse(endDate));
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastHeatCycle = await _response.Content.ReadFromJsonAsync<HeatCycleDto>(JsonOptions);
    }

    [When(@"I record a heat cycle for ""(.*)"" starting on (.*) ending on (.*) with note ""(.*)""")]
    public async Task WhenIRecordAHeatCycleWithNote(
        string patientName, string startDate, string endDate, string note)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new RecordHeatCycleRequest(
            DateOnly.Parse(startDate),
            DateOnly.Parse(endDate),
            note);
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastHeatCycle = await _response.Content.ReadFromJsonAsync<HeatCycleDto>(JsonOptions);
    }

    [When(@"I view the heat cycle history of ""(.*)""")]
    public async Task WhenIViewTheHeatCycleHistoryOf(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        _response = await _client.GetAsync($"/api/v1/patients/{patientId}/heat-cycles");
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
        {
            var pagedResult = await _response.Content.ReadFromJsonAsync<HeatCyclePagedResultDto>(JsonOptions);
            _heatCycleList = pagedResult?.Items.ToList();
        }
    }

    [When(@"I request the predicted next heat for ""(.*)""")]
    public async Task WhenIRequestThePredictedNextHeatFor(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        _response = await _client.GetAsync($"/api/v1/patients/{patientId}/heat-cycles/prediction");
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _prediction = await _response.Content.ReadFromJsonAsync<HeatPredictionDto>(JsonOptions);
    }

    [When(@"I attempt to record a heat cycle for ""(.*)"" starting on (.*) ending on (.*)")]
    public async Task WhenIAttemptToRecordAHeatCycle(string patientName, string startDate, string endDate)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new RecordHeatCycleRequest(
            DateOnly.Parse(startDate),
            DateOnly.Parse(endDate));
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I attempt to record a heat cycle for ""(.*)""")]
    public async Task WhenIAttemptToRecordAHeatCycleFor(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new RecordHeatCycleRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            DateOnly.FromDateTime(DateTime.UtcNow));
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/heat-cycles", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I attempt to view the heat cycle history of ""(.*)""")]
    public async Task WhenIAttemptToViewTheHeatCycleHistoryOf(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        _response = await _client.GetAsync($"/api/v1/patients/{patientId}/heat-cycles");
        _ctx.Set(_response, "LastResponse");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the heat cycle is recorded for ""(.*)""")]
    public void ThenTheHeatCycleIsRecordedFor(string patientName)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastHeatCycle.Should().NotBeNull();
        var patientIds = GetPatientIds();
        _lastHeatCycle!.PatientId.Should().Be(patientIds[patientName]);
    }

    [Then(@"I see (\d+) cycles in chronological order")]
    public void ThenISeeCyclesInChronologicalOrder(int expectedCount)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _heatCycleList.Should().NotBeNull();
        _heatCycleList!.Count.Should().Be(expectedCount);

        // Verify chronological order
        for (int i = 0; i < _heatCycleList.Count - 1; i++)
        {
            _heatCycleList[i].StartDate.Should()
                .BeOnOrBefore(_heatCycleList[i + 1].StartDate);
        }
    }

    [Then(@"the system predicts the next heat around (.*)")]
    public void ThenTheSystemPredictsTheNextHeatAround(string expectedDate)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _prediction.Should().NotBeNull();

        var expected = DateOnly.Parse(expectedDate);
        var diff = Math.Abs(_prediction!.PredictedNextStart.DayNumber - expected.DayNumber);
        diff.Should().BeLessThanOrEqualTo(15,
            $"Predicted next heat should be around {expectedDate}");
    }

    [Then(@"the average cycle interval is approximately (\d+) days")]
    public void ThenTheAverageCycleIntervalIsApproximatelyNDays(int expectedDays)
    {
        _prediction.Should().NotBeNull();
        var diff = Math.Abs(_prediction!.AverageCycleIntervalDays - expectedDays);
        diff.Should().BeLessThanOrEqualTo(10,
            $"Average cycle interval should be approximately {expectedDays} days");
    }

    [Then(@"the heat cycle is recorded with the note")]
    public void ThenTheHeatCycleIsRecordedWithTheNote()
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastHeatCycle.Should().NotBeNull();
        _lastHeatCycle!.Notes.Should().NotBeNullOrEmpty();
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Dictionary<string, Guid> GetPatientIds()
    {
        if (_ctx.ContainsKey("PatientIds"))
            return _ctx.Get<Dictionary<string, Guid>>("PatientIds");
        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "PatientIds");
        return dict;
    }
}
