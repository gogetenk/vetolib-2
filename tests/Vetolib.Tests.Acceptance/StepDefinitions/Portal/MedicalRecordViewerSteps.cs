using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Contracts;
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
    private Guid _ownerAccountId;

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

    [Given(@"a clinic ""([^""]*)"" exists with animals registered")]
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

    [Given(@"an owner ""([^""]*)"" has a portal account linked to ""([^""]*)""")]
    public async Task GivenAnOwnerHasPortalAccountLinkedTo(string ownerName, string clinicName)
    {
        _ctx.Set(ownerName, "CurrentOwnerName");
        await RegisterAndAuthenticateOwner(ownerName);
    }

    [Given(@"""([^""]*)"" has a cat ""([^""]*)"" and a dog ""([^""]*)"" at ""([^""]*)""")]
    public async Task GivenOwnerHasCatAndDogAtClinic(
        string ownerName, string catName, string dogName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, catName, Species.Cat, linkToPortalAccount: true);
        await SeedOwnerWithPatient(ownerName, clinicName, dogName, Species.Dog, linkToPortalAccount: true);
    }

    [Given(@"""([^""]*)"" has a cat ""([^""]*)"" at ""([^""]*)""")]
    public async Task GivenOwnerHasCatAtClinic(string ownerName, string catName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, catName, Species.Cat, linkToPortalAccount: true);
    }

    [Given(@"""([^""]*)"" has (\d+) medical records")]
    public async Task GivenAnimalHasMedicalRecords(string animalName, int recordCount)
    {
        var patientId = _patientIds[animalName];
        var clinicId = TestClinicContext.TestClinicGuid;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        for (int i = 0; i < recordCount; i++)
        {
            var recordResult = MedicalRecord.Create(
                clinicId, patientId,
                $"Diagnosis {i + 1}", $"Treatment {i + 1}",
                "Dr. Test Vet",
                DateTime.UtcNow.AddDays(-(recordCount - i)));
            recordResult.IsSuccess.Should().BeTrue();
            db.MedicalRecords.Add(recordResult.Value);
        }

        await db.SaveChangesAsync();
    }

    [Given(@"""([^""]*)"" has an active prescription for ""([^""]*)""")]
    public async Task GivenAnimalHasActivePrescription(string animalName, string medication)
    {
        var patientId = _patientIds[animalName];
        var clinicId = TestClinicContext.TestClinicGuid;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Create a visible medical record with the prescription
        var recordResult = MedicalRecord.Create(
            clinicId, patientId,
            "Prescription diagnosis", "Prescription treatment",
            "Dr. Test Vet",
            DateTime.UtcNow.AddDays(-1));
        recordResult.IsSuccess.Should().BeTrue();

        var prescriptionResult = Prescription.Create(
            clinicId, recordResult.Value.Id,
            medication, "500mg twice daily", "VET-LICENSE-001");
        prescriptionResult.IsSuccess.Should().BeTrue();

        recordResult.Value.AddPrescription(prescriptionResult.Value);

        db.MedicalRecords.Add(recordResult.Value);
        await db.SaveChangesAsync();
    }

    [Given(@"""([^""]*)"" has a vaccination record for ""([^""]*)""")]
    public async Task GivenAnimalHasVaccinationRecord(string animalName, string vaccineName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = TestClinicContext.TestClinicGuid;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // A vaccination is a medical record with a prescription (the vaccine)
        var recordResult = MedicalRecord.Create(
            clinicId, patientId,
            "Vaccination", $"Administered {vaccineName}",
            "Dr. Test Vet",
            DateTime.UtcNow.AddDays(-7));
        recordResult.IsSuccess.Should().BeTrue();

        var prescriptionResult = Prescription.Create(
            clinicId, recordResult.Value.Id,
            vaccineName, "1 dose", "VET-LICENSE-001");
        prescriptionResult.IsSuccess.Should().BeTrue();

        recordResult.Value.AddPrescription(prescriptionResult.Value);

        db.MedicalRecords.Add(recordResult.Value);
        await db.SaveChangesAsync();
    }

    [Given(@"""([^""]*)"" has weight entries recorded")]
    public async Task GivenAnimalHasWeightEntriesRecorded(string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = TestClinicContext.TestClinicGuid;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var weights = new[] { 3.5m, 3.8m, 4.1m };
        for (int i = 0; i < weights.Length; i++)
        {
            var entryResult = WeightEntry.Create(
                clinicId, patientId, weights[i],
                "Dr. Test Vet", $"Weight check {i + 1}",
                DateTime.UtcNow.AddMonths(-(weights.Length - i)));
            entryResult.IsSuccess.Should().BeTrue();
            db.WeightEntries.Add(entryResult.Value);
        }

        await db.SaveChangesAsync();
    }

    [Given(@"another owner ""([^""]*)"" has a dog ""([^""]*)"" at ""([^""]*)""")]
    public async Task GivenAnotherOwnerHasDogAtClinic(string ownerName, string dogName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, dogName, Species.Dog, linkToPortalAccount: false);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"she views her animals on the portal")]
    public async Task WhenSheViewsHerAnimalsOnThePortal()
    {
        _response = await _client.GetAsync("/api/v1/portal/my-animals");
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

    [When(@"""([^""]*)"" views her animals on the portal")]
    public async Task WhenNamedOwnerViewsHerAnimalsOnThePortal(string ownerName)
    {
        _response = await _client.GetAsync("/api/v1/portal/my-animals");
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

    [When(@"she views the medical records for ""([^""]*)""")]
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

    [When(@"she views the prescriptions for ""([^""]*)""")]
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

    [When(@"she views the vaccinations for ""([^""]*)""")]
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

    [When(@"she views the weight history for ""([^""]*)""")]
    public async Task WhenSheViewsWeightHistoryFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/portal/animals/{patientId}/weight");
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
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _animalList.Should().NotBeNull();
        _animalList.Should().HaveCount(count);
    }

    [Then(@"the list should include ""([^""]*)"" and ""([^""]*)""")]
    public void ThenTheListShouldIncludeAnimals(string name1, string name2)
    {
        _animalList.Should().NotBeNull();
        _animalList!.Select(a => a.Name).Should().Contain(new[] { name1, name2 });
    }

    [Then(@"she should see (\d+) medical records")]
    public void ThenSheShouldSeeNMedicalRecords(int count)
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _medicalRecords.Should().NotBeNull();
        _medicalRecords.Should().HaveCount(count);
    }

    [Then(@"she should see a prescription for ""([^""]*)""")]
    public void ThenSheShouldSeePrescriptionFor(string medication)
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _prescriptions.Should().NotBeNull();
        _prescriptions!.Should().Contain(p => p.Medication == medication);
    }

    [Then(@"she should see a vaccination for ""([^""]*)""")]
    public void ThenSheShouldSeeVaccinationFor(string vaccineName)
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _vaccinations.Should().NotBeNull();
        _vaccinations!.Should().Contain(v => v.Medication == vaccineName);
    }

    [Then(@"she should see the weight entries")]
    public void ThenSheShouldSeeTheWeightEntries()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _weightEntries.Should().NotBeNull();
        _weightEntries.Should().NotBeEmpty();
    }

    [Then(@"she should not see ""([^""]*)"" in her animal list")]
    public void ThenSheShouldNotSeeAnimalInList(string animalName)
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {_response.StatusCode}. Body: {_errorResponseBody}");
        _animalList.Should().NotBeNull();
        _animalList!.Should().NotContain(a => a.Name == animalName);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task RegisterAndAuthenticateOwner(string ownerName)
    {
        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var email = $"{firstName.ToLowerInvariant()}-viewer@portal-test.com";
        var phone = $"+97150{new Random().Next(1000000, 9999999)}";
        var password = "SecurePass1!";

        // Register via the portal API
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/portal/register", new
        {
            Email = email,
            Phone = phone,
            FullName = ownerName,
            Password = password
        });
        registerResponse.IsSuccessStatusCode.Should().BeTrue(
            $"Portal registration should succeed but got {registerResponse.StatusCode}: {await registerResponse.Content.ReadAsStringAsync()}");

        var account = await registerResponse.Content.ReadFromJsonAsync<OwnerAccountDto>(JsonOptions);
        account.Should().NotBeNull();
        _ownerAccountId = account!.Id;

        // Login to get JWT
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/portal/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.IsSuccessStatusCode.Should().BeTrue(
            $"Portal login should succeed but got {loginResponse.StatusCode}: {await loginResponse.Content.ReadAsStringAsync()}");

        var token = await loginResponse.Content.ReadFromJsonAsync<OwnerPortalTokenDto>(JsonOptions);
        token.Should().NotBeNull();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

    private async Task SeedOwnerWithPatient(
        string ownerName, string clinicName, string animalName, Species species, bool linkToPortalAccount = false)
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

            // Link the owner to the portal account so the handler's ownership check passes
            if (linkToPortalAccount && _ownerAccountId != Guid.Empty)
            {
                ownerResult.Value.LinkOwnerAccount(_ownerAccountId);
            }

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
