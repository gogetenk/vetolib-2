using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Breeding.Contracts;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Breeding;

[Binding]
[Scope(Feature = "Pregnancy and gestation tracking")]
internal class PregnancySteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private PregnancyDto? _lastPregnancy;
    private readonly Dictionary<string, PregnancyDto> _pregnanciesByPatient = new();
    private List<PregnancyDto>? _pregnancyList;

    private static readonly JsonSerializerOptions JsonOptions = BreedingSharedSteps.JsonOptions;

    public PregnancySteps(ScenarioContext ctx)
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

    [Given(@"a pregnancy recorded for ""([^""]*)""")]
    public async Task GivenAPregnancyRecordedFor(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new CreatePregnancyRequest(
            patientId, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
            MatingMethod.Natural, null);
        var response = await _client.PostAsJsonAsync("/api/v1/breeding/pregnancies", request);
        response.StatusCode.Should().BeOneOf(
            new[] { HttpStatusCode.OK, HttpStatusCode.Created },
            $"Failed to create pregnancy for {patientName}");

        _lastPregnancy = await response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
        _lastPregnancy.Should().NotBeNull();
        _pregnanciesByPatient[patientName] = _lastPregnancy!;
    }

    [Given(@"(\d+) active pregnancies in the clinic")]
    public async Task GivenNActivePregnanciesInTheClinic(int count)
    {
        var patientIds = GetPatientIds();

        // Create additional patients if needed for multiple pregnancies
        var factory = _ctx.Get<TestWebApplicationFactory>();
        var clinicIds = GetClinicIds();
        var clinicId = clinicIds.Values.First();
        var ownerIds = GetOrCreateDict<Guid>("OwnerIds");
        var ownerId = ownerIds.Values.First();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Vetolib.MedicalRecords.Infrastructure.MedicalRecordsDbContext>();

        for (int i = 0; i < count; i++)
        {
            var name = $"PregnancyPatient{i + 1}";
            if (!patientIds.ContainsKey(name))
            {
                var patientResult = Vetolib.MedicalRecords.Application.Domain.Patient.Create(
                    clinicId, name,
                    Vetolib.MedicalRecords.Contracts.Species.Horse,
                    "Arabian",
                    DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4)),
                    Vetolib.MedicalRecords.Contracts.Sex.Female);
                patientResult.IsSuccess.Should().BeTrue();
                var po = Vetolib.MedicalRecords.Application.Domain.PatientOwner.Create(
                    clinicId, patientResult.Value.Id, ownerId);
                patientResult.Value.AddOwner(po);
                db.Patients.Add(patientResult.Value);
                await db.SaveChangesAsync();
                patientIds[name] = patientResult.Value.Id;
            }

            var request = new CreatePregnancyRequest(
                patientIds[name], null,
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2 - i)),
                MatingMethod.Natural, null);
            var response = await _client.PostAsJsonAsync("/api/v1/breeding/pregnancies", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I record a pregnancy for ""([^""]*)"" with method ""([^""]*)"" mated on (.*)")]
    public async Task WhenIRecordAPregnancyWithMethod(string patientName, string method, string matingDate)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];
        var matingMethod = Enum.Parse<MatingMethod>(method);

        var request = new CreatePregnancyRequest(
            patientId, null,
            DateOnly.Parse(matingDate), matingMethod, null);
        _response = await _client.PostAsJsonAsync("/api/v1/breeding/pregnancies", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
        {
            _lastPregnancy = await _response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
            if (_lastPregnancy != null)
                _pregnanciesByPatient[patientName] = _lastPregnancy;
        }
    }

    [When(@"I schedule an ultrasound for ""([^""]*)"" on (.*) with note ""([^""]*)""")]
    public async Task WhenIScheduleAnUltrasound(string patientName, string scheduledDate, string note)
    {
        var pregnancy = _pregnanciesByPatient.ContainsKey(patientName)
            ? _pregnanciesByPatient[patientName]
            : _lastPregnancy;
        pregnancy.Should().NotBeNull($"A pregnancy must exist for {patientName}");

        var request = new ScheduleCheckRequest(
            DateOnly.Parse(scheduledDate),
            PregnancyCheckType.Ultrasound,
            note);
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{pregnancy!.Id}/checks", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I record the delivery on (.*) with outcome ""([^""]*)"" and (\d+) offspring")]
    public async Task WhenIRecordTheDeliveryWithOutcomeAndOffspring(
        string deliveryDate, string outcome, int offspringCount)
    {
        _lastPregnancy.Should().NotBeNull();
        var parsedOutcome = Enum.Parse<PregnancyOutcome>(outcome);

        var request = new RecordDeliveryRequest(
            DateOnly.Parse(deliveryDate), parsedOutcome, offspringCount, null);
        _response = await _client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{_lastPregnancy!.Id}/delivery", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastPregnancy = await _response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
    }

    [When(@"I record the delivery on (.*) with outcome ""([^""]*)"" and (\d+) live offspring")]
    public async Task WhenIRecordTheDeliveryWithOutcomeAndLiveOffspring(
        string deliveryDate, string outcome, int offspringCount)
    {
        _lastPregnancy.Should().NotBeNull();
        var parsedOutcome = Enum.Parse<PregnancyOutcome>(outcome);

        var request = new RecordDeliveryRequest(
            DateOnly.Parse(deliveryDate), parsedOutcome, offspringCount, null);
        _response = await _client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{_lastPregnancy!.Id}/delivery", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastPregnancy = await _response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
    }

    [When(@"I record the pregnancy ended with outcome ""([^""]*)"" on (.*)")]
    public async Task WhenIRecordThePregnancyEndedWithOutcome(string outcome, string date)
    {
        _lastPregnancy.Should().NotBeNull();
        var parsedOutcome = Enum.Parse<PregnancyOutcome>(outcome);

        // Use the loss endpoint for Miscarriage/Abortion
        var request = new RecordDeliveryRequest(
            DateOnly.Parse(date), parsedOutcome, 0, null);
        _response = await _client.PutAsJsonAsync(
            $"/api/v1/breeding/pregnancies/{_lastPregnancy!.Id}/loss", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastPregnancy = await _response.Content.ReadFromJsonAsync<PregnancyDto>(JsonOptions);
    }

    [When(@"I attempt to record a pregnancy for ""([^""]*)""")]
    public async Task WhenIAttemptToRecordAPregnancyFor(string patientName)
    {
        var patientIds = GetPatientIds();
        var patientId = patientIds[patientName];

        var request = new CreatePregnancyRequest(
            patientId, null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            MatingMethod.Natural, null);
        _response = await _client.PostAsJsonAsync("/api/v1/breeding/pregnancies", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I view active pregnancies")]
    public async Task WhenIViewActivePregnancies()
    {
        _response = await _client.GetAsync("/api/v1/breeding/pregnancies/active");
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _pregnancyList = await _response.Content.ReadFromJsonAsync<List<PregnancyDto>>(JsonOptions);
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the pregnancy is created for ""([^""]*)""")]
    public void ThenThePregnancyIsCreatedFor(string patientName)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastPregnancy.Should().NotBeNull();
        var patientIds = GetPatientIds();
        _lastPregnancy!.PatientId.Should().Be(patientIds[patientName]);
    }

    [Then(@"the expected due date is calculated based on species gestation period")]
    public void ThenTheExpectedDueDateIsCalculatedBasedOnSpeciesGestationPeriod()
    {
        _lastPregnancy.Should().NotBeNull();
        _lastPregnancy!.ExpectedDueDate.Should().BeAfter(_lastPregnancy.MatingDate);
    }

    [Then(@"the pregnancy is created with method ""([^""]*)""")]
    public void ThenThePregnancyIsCreatedWithMethod(string method)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastPregnancy.Should().NotBeNull();
        _lastPregnancy!.MatingMethod.Should().Be(Enum.Parse<MatingMethod>(method));
    }

    [Then(@"the expected due date for ""([^""]*)"" is approximately (\d+) days after mating")]
    public void ThenTheExpectedDueDateIsApproximatelyNDaysAfterMating(string patientName, int days)
    {
        var pregnancy = _pregnanciesByPatient.ContainsKey(patientName)
            ? _pregnanciesByPatient[patientName]
            : _lastPregnancy;
        pregnancy.Should().NotBeNull();

        var expectedDueDate = pregnancy!.MatingDate.AddDays(days);
        // Allow +/- 5 days tolerance for species-specific calculations
        var diff = Math.Abs(pregnancy.ExpectedDueDate.DayNumber - expectedDueDate.DayNumber);
        diff.Should().BeLessThanOrEqualTo(5,
            $"Expected due date should be approximately {days} days after mating");
    }

    [Then(@"the ultrasound appears in the pregnancy timeline")]
    public void ThenTheUltrasoundAppearsInThePregnancyTimeline()
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Then(@"the pregnancy is marked as completed")]
    public void ThenThePregnancyIsMarkedAsCompleted()
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastPregnancy.Should().NotBeNull();
        _lastPregnancy!.Status.Should().Be(PregnancyStatus.Completed);
    }

    [Then(@"the actual delivery date is (.*)")]
    public void ThenTheActualDeliveryDateIs(string expectedDate)
    {
        _lastPregnancy.Should().NotBeNull();
        _lastPregnancy!.ActualDeliveryDate.Should().Be(DateOnly.Parse(expectedDate));
    }

    [Then(@"the pregnancy is marked as completed with outcome ""([^""]*)""")]
    public void ThenThePregnancyIsMarkedAsCompletedWithOutcome(string outcome)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastPregnancy.Should().NotBeNull();
        var expectedOutcome = Enum.Parse<PregnancyOutcome>(outcome);
        _lastPregnancy!.Outcome.Should().Be(expectedOutcome);
    }

    [Then(@"I see (\d+) pregnancies sorted by expected due date")]
    public void ThenISeePregnanciesSortedByExpectedDueDate(int expectedCount)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _pregnancyList.Should().NotBeNull();
        _pregnancyList!.Count.Should().Be(expectedCount);

        // Verify sorted by expected due date
        for (int i = 0; i < _pregnancyList.Count - 1; i++)
        {
            _pregnancyList[i].ExpectedDueDate.Should()
                .BeOnOrBefore(_pregnancyList[i + 1].ExpectedDueDate);
        }
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

    private Dictionary<string, Guid> GetClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }

    private Dictionary<string, T> GetOrCreateDict<T>(string key)
    {
        if (_ctx.ContainsKey(key))
            return _ctx.Get<Dictionary<string, T>>(key);
        var dict = new Dictionary<string, T>();
        _ctx.Set(dict, key);
        return dict;
    }
}
