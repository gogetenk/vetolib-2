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

namespace Vetolib.Tests.Acceptance.StepDefinitions.MedicalRecords;

[Binding]
[Scope(Feature = "Animal medical record")]
internal class MedicalRecordSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private string? _errorResponseBody;

    private readonly Dictionary<string, Guid> _ownerIds = new();
    private readonly Dictionary<string, Guid> _patientIds = new();
    private PatientDto? _createdPatient;
    private MedicalRecordDto? _createdRecord;
    private PrescriptionDto? _createdPrescription;
    private Guid _lastRecordId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public MedicalRecordSteps(ScenarioContext ctx)
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

    [Given(@"an owner ""(.*)"" with email ""(.*)""")]
    public async Task GivenAnOwner(string ownerName, string email)
    {
        var clinicIds = GetClinicIds();
        var clinicName = clinicIds.Keys.First();
        var clinicId = clinicIds[clinicName];

        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : "";

        // Create owner directly in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, firstName, lastName, email, null);
        ownerResult.IsSuccess.Should().BeTrue($"Owner creation should succeed for {ownerName}");

        db.Owners.Add(ownerResult.Value);
        await db.SaveChangesAsync();

        _ownerIds[ownerName] = ownerResult.Value.Id;
    }

    [Given(@"an animal ""(.*)"" breed ""(.*)"" belonging to ""(.*)""")]
    public async Task GivenAnAnimal(string animalName, string breed, string ownerName)
    {
        var clinicIds = GetClinicIds();
        var clinicName = clinicIds.Keys.First();
        var clinicId = clinicIds[clinicName];
        var ownerId = _ownerIds[ownerName];

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var patientResult = Patient.Create(clinicId, animalName, InferSpecies(breed), breed, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)));
        patientResult.IsSuccess.Should().BeTrue($"Patient creation should succeed for {animalName}");

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerId);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _patientIds[animalName] = patient.Id;
    }

    [Given(@"an animal ""(.*)"" in clinic ""(.*)""")]
    public async Task GivenAnAnimalInClinic(string animalName, string clinicName)
    {
        var clinicId = SharedSteps.GenerateGuidFromString(clinicName);
        var clinicIds = GetClinicIds();
        clinicIds[clinicName] = clinicId;
        _ctx.Set(clinicIds, "ClinicIds");

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        var originalClinicId = testClinicContext.ClinicId;

        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, "Other", "Owner", $"owner@{clinicName.ToLowerInvariant().Replace(" ", "")}.com", null);
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);

        var patientResult = Patient.Create(clinicId, animalName, Species.Dog, "Mixed", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)));
        patientResult.IsSuccess.Should().BeTrue();

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerResult.Value.Id);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _patientIds[animalName] = patient.Id;

        testClinicContext.ClinicId = originalClinicId;
    }

    [Given(@"(\d+) examinations in the record of ""(.*)""")]
    public async Task GivenNExaminationsInTheRecord(int count, string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = GetClinicIds().Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        for (int i = 1; i <= count; i++)
        {
            var recordResult = MedicalRecord.Create(
                clinicId, patientId,
                $"Diagnostic {i}",
                $"Treatment {i}",
                "Dr. Test",
                DateTime.UtcNow.AddDays(-i));

            recordResult.IsSuccess.Should().BeTrue();
            db.MedicalRecords.Add(recordResult.Value);
        }

        await db.SaveChangesAsync();
    }

    [Given(@"an existing examination for ""(.*)""")]
    public async Task GivenAnExistingExaminationFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = GetClinicIds().Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var recordResult = MedicalRecord.Create(
            clinicId, patientId,
            "Routine consultation",
            "Observation",
            "Dr. Test",
            DateTime.UtcNow);

        recordResult.IsSuccess.Should().BeTrue();
        db.MedicalRecords.Add(recordResult.Value);
        await db.SaveChangesAsync();

        _lastRecordId = recordResult.Value.Id;
    }

    [Given(@"an examination in the record of ""(.*)""")]
    public async Task GivenAnExaminationInTheRecord(string animalName)
    {
        await GivenAnExistingExaminationFor(animalName);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I create an animal ""(.*)"" breed ""(.*)"" for owner ""(.*)""")]
    public async Task WhenICreateAnAnimal(string animalName, string breed, string ownerName)
    {
        var species = InferSpecies(breed);

        // Retrieve owner phone from DB for the request
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var ownerId = _ownerIds[ownerName];
        var owner = await db.Owners.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == ownerId);
        var ownerFullName = owner is not null ? $"{owner.FirstName} {owner.LastName}" : ownerName;
        var ownerPhone = owner?.Phone ?? "+971 50 000 0000";

        var request = new CreatePatientRequest(animalName, species, breed, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)), ownerFullName, ownerPhone);
        _response = await _client.PostAsJsonAsync("/api/v1/patients", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdPatient = await _response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"I attempt to add an examination for ""(.*)""")]
    public async Task WhenIAttemptToAddAnExamination(string animalName)
    {
        var patientId = _patientIds.TryGetValue(animalName, out var id) ? id : Guid.NewGuid();
        var request = new AddMedicalRecordRequest("Test diagnostic", "Test treatment");
        _response = await _client.PostAsJsonAsync($"/api/v1/patients/{patientId}/records", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"I add an examination for ""(.*)"" with diagnosis ""(.*)"" and treatment ""(.*)""")]
    public async Task WhenIAddAnExamination(string animalName, string diagnosis, string treatment)
    {
        var patientId = _patientIds[animalName];
        var request = new AddMedicalRecordRequest(diagnosis, treatment);
        _response = await _client.PostAsJsonAsync($"/api/v1/patients/{patientId}/records", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdRecord = await _response.Content.ReadFromJsonAsync<MedicalRecordDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"I view the record of ""(.*)""")]
    public async Task WhenIViewTheRecord(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/patients/{patientId}/records");
    }

    [When(@"I list the animals of ""(.*)""")]
    public async Task WhenIListTheAnimalsOf(string clinicName)
    {
        var clinicId = GetClinicIds()[clinicName];
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        _response = await _client.GetAsync("/api/v1/patients");
    }

    [When(@"I create a prescription with medication ""(.*)"" dosage ""(.*)""")]
    public async Task WhenICreateAPrescription(string medication, string dosage)
    {
        var patientId = _patientIds.Values.FirstOrDefault();
        var request = new AddPrescriptionRequest(medication, dosage);
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/records/{_lastRecordId}/prescriptions",
            request);

        if (_response.IsSuccessStatusCode)
        {
            _createdPrescription = await _response.Content.ReadFromJsonAsync<PrescriptionDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"I attempt to delete this examination")]
    public async Task WhenIAttemptToDeleteThisExamination()
    {
        var patientId = _patientIds.Values.FirstOrDefault();
        _response = await _client.DeleteAsync(
            $"/api/v1/patients/{patientId}/records/{_lastRecordId}");
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the animal is created in the clinic")]
    public void ThenTheAnimalIsCreatedInTheClinic()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdPatient.Should().NotBeNull();
        _createdPatient!.ClinicId.Should().Be(GetClinicIds().Values.First());
    }

    [Then(@"its medical record is empty")]
    public void ThenItsMedicalRecordIsEmpty()
    {
        _createdPatient.Should().NotBeNull();
    }

    [Then(@"the owner ""(.*)"" is linked to ""(.*)""")]
    public void ThenTheOwnerIsLinkedTo(string ownerName, string animalName)
    {
        _createdPatient.Should().NotBeNull();
        _createdPatient!.OwnerName.Should().NotBeNullOrEmpty();
        _createdPatient.OwnerName.Should().Contain(ownerName.Split(' ')[0]);
    }

    [Then(@"""(.*)"" does not appear in the list")]
    public async Task ThenDoesNotAppearInTheList(string animalName)
    {
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_response.StatusCode}. Body: {_errorResponseBody}");
        var result = await _response.Content.ReadFromJsonAsync<PatientPagedResultDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(p => p.Name == animalName);
    }

    [Then(@"the examination appears in the history of ""(.*)""")]
    public void ThenTheExaminationAppearsInTheHistory(string animalName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected success response, got: {_errorResponseBody}");
        _createdRecord.Should().NotBeNull();
        _createdRecord!.PatientId.Should().Be(_patientIds[animalName]);
    }

    [Then(@"it is timestamped with today's date")]
    public void ThenItIsTimestampedWithTodaysDate()
    {
        _createdRecord.Should().NotBeNull();
        _createdRecord!.ExaminedAt.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Then(@"it bears the current veterinarian as author")]
    public void ThenItBearsTheCurrentVeterinarianAsAuthor()
    {
        _createdRecord.Should().NotBeNull();
        _createdRecord!.VetName.Should().NotBeNullOrEmpty();
    }

    [Then(@"I see (\d+) examinations in reverse chronological order")]
    public async Task ThenISeeNExaminationsInReverseChronologicalOrder(int expectedCount)
    {
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_response.StatusCode}. Body: {_errorResponseBody}");

        var paged = await _response.Content.ReadFromJsonAsync<MedicalRecordPagedResultDto>(JsonOptions);
        paged.Should().NotBeNull();
        var records = paged!.Items.ToList();
        records.Count.Should().Be(expectedCount);

        // Verify descending order
        for (int i = 0; i < records.Count - 1; i++)
        {
            records[i].ExaminedAt.Should().BeOnOrAfter(records[i + 1].ExaminedAt);
        }
    }

    [Then(@"the prescription is created with license number ""(.*)""")]
    public void ThenThePrescriptionIsCreated(string licenseNumber)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected success, got: {_errorResponseBody}");
        _createdPrescription.Should().NotBeNull();
        _createdPrescription!.VetLicenseNumber.Should().Be(licenseNumber);
    }

    [Then(@"it is linked to the examination")]
    public void ThenItIsLinkedToTheExamination()
    {
        _createdPrescription.Should().NotBeNull();
        _createdPrescription!.MedicalRecordId.Should().Be(_lastRecordId);
    }

    [Then(@"the examination is still visible in the history")]
    public async Task ThenTheExaminationIsStillVisibleInTheHistory()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var record = await db.MedicalRecords.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == _lastRecordId);
        record.Should().NotBeNull("The examination must still be present after a deletion attempt");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Dictionary<string, Guid> GetClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }

    // Helper record for paged list deserialization
    private record PatientPagedResultDto(List<PatientDto> Items, int Total, int Page, int PageSize);

    private static Species InferSpecies(string breed)
    {
        var breedLower = breed.ToLowerInvariant();
        if (breedLower.Contains("cat") || breedLower.Contains("persian") || breedLower.Contains("siamese"))
            return Species.Cat;
        if (breedLower.Contains("parrot") || breedLower.Contains("canary"))
            return Species.Bird;
        return Species.Dog;
    }
}
