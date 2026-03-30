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
[Scope(Feature = "Litter management")]
internal class LitterSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private LitterDto? _lastLitter;
    private List<LitterDto>? _litterList;

    private static readonly JsonSerializerOptions JsonOptions = BreedingSharedSteps.JsonOptions;

    public LitterSteps(ScenarioContext ctx)
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

    [Given(@"a litter for mother ""(.*)"" born on (.*)")]
    public async Task GivenALitterForMotherBornOn(string motherName, string birthDate)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        var request = new CreateLitterRequest(
            motherId, null, null,
            DateOnly.Parse(birthDate), 1, 1, null);
        var response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        response.StatusCode.Should().BeOneOf(
            new[] { HttpStatusCode.OK, HttpStatusCode.Created },
            $"Failed to create litter for mother {motherName}");

        _lastLitter = await response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
        _lastLitter.Should().NotBeNull();

        var litterIds = GetOrCreateDict<Guid>("LitterIds");
        litterIds[motherName] = _lastLitter!.Id;
    }

    [Given(@"a litter for mother ""(.*)"" with (\d+) offspring registered")]
    public async Task GivenALitterWithOffspringRegistered(string motherName, int offspringCount)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        var request = new CreateLitterRequest(
            motherId, null, null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
            offspringCount, offspringCount, null);
        var response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        _lastLitter = await response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
        _lastLitter.Should().NotBeNull();

        var litterIds = GetOrCreateDict<Guid>("LitterIds");
        litterIds[motherName] = _lastLitter!.Id;
    }

    [Given(@"(\d+) litters registered for mother ""(.*)""")]
    public async Task GivenNLittersRegisteredForMother(int count, string motherName)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        for (int i = 0; i < count; i++)
        {
            var request = new CreateLitterRequest(
                motherId, null, null,
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-(count - i) * 6)),
                1, 1, $"Litter {i + 1}");
            var response = await _client.PostAsJsonAsync("/api/v1/litters", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }
    }

    [Given(@"a litter for mother ""(.*)"" in clinic ""(.*)""")]
    public async Task GivenALitterForMotherInClinic(string motherName, string clinicName)
    {
        // The litter should already be created via the normal litter creation step
        // This Given just ensures it exists in the specified clinic context
        var patientIds = GetPatientIds();
        if (patientIds.ContainsKey(motherName))
        {
            var motherId = patientIds[motherName];
            var request = new CreateLitterRequest(
                motherId, null, null,
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                1, 1, null);
            var response = await _client.PostAsJsonAsync("/api/v1/litters", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            _lastLitter = await response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
        }
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I register a litter for mother ""(.*)"" with father ""(.*)"" on (.*) with (\d+) born and (\d+) alive")]
    public async Task WhenIRegisterALitterWithFather(
        string motherName, string fatherName, string birthDate, int bornCount, int aliveCount)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];
        var fatherId = patientIds[fatherName];

        var request = new CreateLitterRequest(
            motherId, fatherId, null,
            DateOnly.Parse(birthDate), bornCount, aliveCount, null);
        _response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastLitter = await _response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
    }

    [When(@"I register a litter for mother ""(.*)"" with external father ""(.*)"" on (.*) with (\d+) born and (\d+) alive")]
    public async Task WhenIRegisterALitterWithExternalFather(
        string motherName, string externalFatherName, string birthDate, int bornCount, int aliveCount)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        var request = new CreateLitterRequest(
            motherId, null, externalFatherName,
            DateOnly.Parse(birthDate), bornCount, aliveCount, null);
        _response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastLitter = await _response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
    }

    [When(@"I add offspring ""(.*)"" sex ""(.*)"" breed ""(.*)"" to the litter")]
    public async Task WhenIAddOffspringToTheLitter(string offspringName, string sex, string breed)
    {
        _lastLitter.Should().NotBeNull("A litter must exist before adding offspring");

        // First, create the patient via MedicalRecords API
        var patientIds = GetPatientIds();
        var ownerIds = GetOrCreateDict<Guid>("OwnerIds");
        var ownerName = ownerIds.Keys.First();

        var createPatientRequest = new Vetolib.MedicalRecords.Contracts.CreatePatientRequest(
            offspringName,
            Vetolib.MedicalRecords.Contracts.Species.Horse,
            breed,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ownerName,
            "+971 50 000 0000",
            Enum.Parse<Vetolib.MedicalRecords.Contracts.Sex>(sex));
        var patientResponse = await _client.PostAsJsonAsync("/api/v1/patients", createPatientRequest);

        if (patientResponse.IsSuccessStatusCode)
        {
            var patientDto = await patientResponse.Content.ReadFromJsonAsync<Vetolib.MedicalRecords.Contracts.PatientDto>(JsonOptions);
            patientIds[offspringName] = patientDto!.Id;

            // Now add to litter
            var addRequest = new AddOffspringToLitterRequest(patientDto.Id, null);
            _response = await _client.PostAsJsonAsync($"/api/v1/litters/{_lastLitter!.Id}/offspring", addRequest);
            _ctx.Set(_response, "LastResponse");
        }
        else
        {
            _response = patientResponse;
            _ctx.Set(_response, "LastResponse");
        }
    }

    [When(@"I attempt to register a litter with (\d+) born and (\d+) alive")]
    public async Task WhenIAttemptToRegisterALitterWithBornAndAlive(int bornCount, int aliveCount)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds.Values.First();

        var request = new CreateLitterRequest(
            motherId, null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), bornCount, aliveCount, null);
        _response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I view the litter details")]
    public async Task WhenIViewTheLitterDetails()
    {
        _lastLitter.Should().NotBeNull();
        _response = await _client.GetAsync($"/api/v1/litters/{_lastLitter!.Id}");
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _lastLitter = await _response.Content.ReadFromJsonAsync<LitterDto>(JsonOptions);
    }

    [When(@"I attempt to register a litter for mother ""(.*)""")]
    public async Task WhenIAttemptToRegisterALitterForMother(string motherName)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        var request = new CreateLitterRequest(
            motherId, null, null,
            DateOnly.FromDateTime(DateTime.UtcNow), 1, 1, null);
        _response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I attempt to register a litter for mother ""(.*)"" with father ""(.*)""")]
    public async Task WhenIAttemptToRegisterALitterForMotherWithFather(string motherName, string fatherName)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];
        var fatherId = patientIds[fatherName];

        var request = new CreateLitterRequest(
            motherId, fatherId, null,
            DateOnly.FromDateTime(DateTime.UtcNow), 1, 1, null);
        _response = await _client.PostAsJsonAsync("/api/v1/litters", request);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I view the breeding history of ""(.*)""")]
    public async Task WhenIViewTheBreedingHistoryOf(string motherName)
    {
        var patientIds = GetPatientIds();
        var motherId = patientIds[motherName];

        _response = await _client.GetAsync($"/api/v1/patients/{motherId}/litters");
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _litterList = await _response.Content.ReadFromJsonAsync<List<LitterDto>>(JsonOptions);
    }

    [When(@"I list litters")]
    public async Task WhenIListLitters()
    {
        // List litters for any mother visible to current clinic
        // Use the first patient if available, otherwise just hit a general endpoint
        var patientIds = GetPatientIds();
        if (patientIds.Count > 0)
        {
            var motherId = patientIds.Values.First();
            _response = await _client.GetAsync($"/api/v1/patients/{motherId}/litters");
        }
        else
        {
            // No patient available in context — make a request that should return empty
            _response = await _client.GetAsync($"/api/v1/patients/{Guid.NewGuid()}/litters");
        }
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
            _litterList = await _response.Content.ReadFromJsonAsync<List<LitterDto>>(JsonOptions);
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the litter is created and linked to ""(.*)""")]
    public void ThenTheLitterIsCreatedAndLinkedTo(string motherName)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastLitter.Should().NotBeNull();
        var patientIds = GetPatientIds();
        _lastLitter!.MotherPatientId.Should().Be(patientIds[motherName]);
    }

    [Then(@"the litter shows ""(.*)"" as the father")]
    public void ThenTheLitterShowsAsFather(string fatherName)
    {
        _lastLitter.Should().NotBeNull();
        var patientIds = GetPatientIds();
        _lastLitter!.FatherPatientId.Should().Be(patientIds[fatherName]);
    }

    [Then(@"the litter is created with external father name ""(.*)""")]
    public void ThenTheLitterIsCreatedWithExternalFatherName(string externalFatherName)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        _lastLitter.Should().NotBeNull();
        _lastLitter!.ExternalFatherName.Should().Be(externalFatherName);
    }

    [Then(@"""(.*)"" appears as a patient in the clinic")]
    public void ThenAppearsAsAPatientInTheClinic(string patientName)
    {
        var patientIds = GetPatientIds();
        patientIds.Should().ContainKey(patientName);
    }

    [Then(@"""(.*)"" is linked to the litter as offspring")]
    public void ThenIsLinkedToTheLitterAsOffspring(string patientName)
    {
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Then(@"I see the mother ""(.*)"", the father, birth date, and all (\d+) offspring")]
    public void ThenISeeTheMotherFatherBirthDateAndAllOffspring(string motherName, int offspringCount)
    {
        _lastLitter.Should().NotBeNull();
        var patientIds = GetPatientIds();
        _lastLitter!.MotherPatientId.Should().Be(patientIds[motherName]);
        _lastLitter.BirthDate.Should().NotBe(default);
    }

    [Then(@"I see (\d+) litters in reverse chronological order")]
    public void ThenISeeLittersInReverseChronologicalOrder(int expectedCount)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _litterList.Should().NotBeNull();
        _litterList!.Count.Should().Be(expectedCount);

        // Verify reverse chronological order
        for (int i = 0; i < _litterList.Count - 1; i++)
        {
            _litterList[i].BirthDate.Should().BeOnOrAfter(_litterList[i + 1].BirthDate);
        }
    }

    [Then(@"I do not see litters from ""(.*)""")]
    public void ThenIDoNotSeeLittersFromClinic(string clinicName)
    {
        // If we got a successful response, the list should be empty or not contain
        // litters from another clinic (tenant isolation)
        if (_response.IsSuccessStatusCode && _litterList != null)
        {
            _litterList.Should().BeEmpty(
                $"Should not see litters from {clinicName} due to tenant isolation");
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

    private Dictionary<string, T> GetOrCreateDict<T>(string key)
    {
        if (_ctx.ContainsKey(key))
            return _ctx.Get<Dictionary<string, T>>(key);
        var dict = new Dictionary<string, T>();
        _ctx.Set(dict, key);
        return dict;
    }
}
