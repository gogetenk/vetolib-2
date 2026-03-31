using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Portal;

[Binding]
[Scope(Feature = "Medical Record Viewer")]
internal class MedicalRecordViewerSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage? _response;
    private string? _errorResponseBody;

    private List<PortalAnimalDto>? _animalList;
    private List<PortalMedicalRecordDto>? _medicalRecords;
    private List<PortalPrescriptionDto>? _prescriptions;
    private List<PortalVaccinationDto>? _vaccinations;
    private List<PortalWeightEntryDto>? _weightEntries;

    private readonly Dictionary<string, Guid> _ownerIds = new();
    private readonly Dictionary<string, Guid> _patientIds = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public MedicalRecordViewerSteps(ScenarioContext ctx)
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

    [Given(@"a clinic ""(.*)"" exists with animals registered")]
    public void GivenAClinicExistsWithAnimalsRegistered(string clinicName)
    {
        var clinicIds = GetOrCreateClinicIds();
        var clinicId = clinicIds.Count == 0
            ? TestClinicContext.TestClinicGuid
            : SharedSteps.GenerateGuidFromString(clinicName);
        clinicIds[clinicName] = clinicId;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = TestClinicContext.TestClinicGuid;

        _ctx.Set(clinicIds, "ClinicIds");
    }

    [Given(@"an owner ""(.*)"" has a portal account linked to ""(.*)""")]
    public void GivenAnOwnerHasPortalAccountLinkedTo(string ownerName, string clinicName)
    {
        // Seed a portal account linked to the clinic
        // For now, just store the owner name for later steps
        _ctx.Set(ownerName, "CurrentOwnerName");
    }

    [Given(@"""(.*)"" has a cat ""(.*)"" and a dog ""(.*)"" at ""(.*)""")]
    public async Task GivenOwnerHasCatAndDogAtClinic(
        string ownerName, string catName, string dogName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, catName, Species.Cat);
        await SeedOwnerWithPatient(ownerName, clinicName, dogName, Species.Dog);
    }

    [Given(@"""(.*)"" has a cat ""(.*)"" at ""(.*)""")]
    public async Task GivenOwnerHasCatAtClinic(string ownerName, string catName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, catName, Species.Cat);
    }

    [Given(@"""(.*)"" has (\d+) medical records")]
    public void GivenAnimalHasMedicalRecords(string animalName, int recordCount)
    {
        // Seed medical records for the animal
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has an active prescription for ""(.*)""")]
    public void GivenAnimalHasActivePrescription(string animalName, string medication)
    {
        // Seed a prescription for the animal
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has a vaccination record for ""(.*)""")]
    public void GivenAnimalHasVaccinationRecord(string animalName, string vaccineName)
    {
        // Seed a vaccination record for the animal
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has weight entries recorded")]
    public void GivenAnimalHasWeightEntriesRecorded(string animalName)
    {
        // Seed weight entries for the animal
        throw new PendingStepException();
    }

    [Given(@"another owner ""(.*)"" has a dog ""(.*)"" at ""(.*)""")]
    public async Task GivenAnotherOwnerHasDogAtClinic(string ownerName, string dogName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, dogName, Species.Dog);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"she views her animals on the portal")]
    public async Task WhenSheViewsHerAnimalsOnThePortal()
    {
        _response = await _client.GetAsync("/api/v1/portal/animals");
        if (_response.IsSuccessStatusCode)
        {
            _animalList = await _response.Content
                .ReadFromJsonAsync<List<PortalAnimalDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"""(.*)"" views her animals on the portal")]
    public async Task WhenNamedOwnerViewsHerAnimalsOnThePortal(string ownerName)
    {
        _response = await _client.GetAsync("/api/v1/portal/animals");
        if (_response.IsSuccessStatusCode)
        {
            _animalList = await _response.Content
                .ReadFromJsonAsync<List<PortalAnimalDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views the medical records for ""(.*)""")]
    public async Task WhenSheViewsMedicalRecordsFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/portal/animals/{patientId}/records");
        if (_response.IsSuccessStatusCode)
        {
            _medicalRecords = await _response.Content
                .ReadFromJsonAsync<List<PortalMedicalRecordDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views the prescriptions for ""(.*)""")]
    public async Task WhenSheViewsPrescriptionsFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/portal/animals/{patientId}/prescriptions");
        if (_response.IsSuccessStatusCode)
        {
            _prescriptions = await _response.Content
                .ReadFromJsonAsync<List<PortalPrescriptionDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views the vaccinations for ""(.*)""")]
    public async Task WhenSheViewsVaccinationsFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/portal/animals/{patientId}/vaccinations");
        if (_response.IsSuccessStatusCode)
        {
            _vaccinations = await _response.Content
                .ReadFromJsonAsync<List<PortalVaccinationDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views the weight history for ""(.*)""")]
    public async Task WhenSheViewsWeightHistoryFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/portal/animals/{patientId}/weight-history");
        if (_response.IsSuccessStatusCode)
        {
            _weightEntries = await _response.Content
                .ReadFromJsonAsync<List<PortalWeightEntryDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"she should see (\d+) animals listed")]
    public void ThenSheShouldSeeNAnimalsListed(int count)
    {
        _animalList.Should().NotBeNull();
        _animalList.Should().HaveCount(count);
    }

    [Then(@"the list should include ""(.*)"" and ""(.*)""")]
    public void ThenTheListShouldIncludeAnimals(string name1, string name2)
    {
        _animalList.Should().NotBeNull();
        _animalList!.Select(a => a.Name).Should().Contain(new[] { name1, name2 });
    }

    [Then(@"she should see (\d+) medical records")]
    public void ThenSheShouldSeeNMedicalRecords(int count)
    {
        _medicalRecords.Should().NotBeNull();
        _medicalRecords.Should().HaveCount(count);
    }

    [Then(@"she should see a prescription for ""(.*)""")]
    public void ThenSheShouldSeePrescriptionFor(string medication)
    {
        _prescriptions.Should().NotBeNull();
        _prescriptions!.Should().Contain(p => p.Medication == medication);
    }

    [Then(@"she should see a vaccination for ""(.*)""")]
    public void ThenSheShouldSeeVaccinationFor(string vaccineName)
    {
        _vaccinations.Should().NotBeNull();
        _vaccinations!.Should().Contain(v => v.Medication == vaccineName);
    }

    [Then(@"she should see the weight entries")]
    public void ThenSheShouldSeeTheWeightEntries()
    {
        _weightEntries.Should().NotBeNull();
        _weightEntries.Should().NotBeEmpty();
    }

    [Then(@"she should not see ""(.*)"" in her animal list")]
    public void ThenSheShouldNotSeeAnimalInList(string animalName)
    {
        _animalList.Should().NotBeNull();
        _animalList!.Should().NotContain(a => a.Name == animalName);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task SeedOwnerWithPatient(
        string ownerName, string clinicName, string animalName, Species species)
    {
        var clinicIds = GetOrCreateClinicIds();
        if (!clinicIds.TryGetValue(clinicName, out var clinicId))
        {
            clinicId = TestClinicContext.TestClinicGuid;
        }

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Ensure owner exists
        if (!_ownerIds.ContainsKey(ownerName))
        {
            var names = ownerName.Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";
            var email = $"{firstName.ToLowerInvariant()}@portal-test.com";

            var ownerResult = Owner.Create(clinicId, firstName, lastName, email, null);
            ownerResult.IsSuccess.Should().BeTrue($"Owner creation should succeed for {ownerName}");

            db.Owners.Add(ownerResult.Value);
            await db.SaveChangesAsync();

            _ownerIds[ownerName] = ownerResult.Value.Id;
        }

        var ownerId = _ownerIds[ownerName];

        var patientResult = Patient.Create(
            clinicId, animalName, species, "Mixed",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)));
        patientResult.IsSuccess.Should().BeTrue($"Patient creation should succeed for {animalName}");

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerId);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _patientIds[animalName] = patient.Id;
    }

    private Dictionary<string, Guid> GetOrCreateClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");

        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }
}
