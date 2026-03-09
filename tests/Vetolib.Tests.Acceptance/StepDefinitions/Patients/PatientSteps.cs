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
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Patients;

[Binding]
[Scope(Feature = "Patient standalone CRUD")]
internal class PatientSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private PatientDto? _patientDto;
    private List<PatientDto>? _patientList;
    private PatientDetailDto? _patientDetailDto;
    private Guid _lastPatientId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PatientSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN steps ─────────────────────────────────────────────

    [Given(@"3 patients exist including one named ""(.*)""")]
    public async Task Given3PatientsExist(string namedPatient)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var names = new[] { namedPatient, "Beta", "Gamma" };
        foreach (var name in names)
        {
            var result = Patient.Create(clinicId, name, Species.Dog, "Mixed", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)));
            result.IsSuccess.Should().BeTrue();
            var owner = Owner.Create(clinicId, name + "Owner", "Family", $"{name.ToLowerInvariant()}@test.com", null);
            owner.IsSuccess.Should().BeTrue();
            db.Owners.Add(owner.Value);
            var po = PatientOwner.Create(clinicId, result.Value.Id, owner.Value.Id);
            result.Value.AddOwner(po);
            db.Patients.Add(result.Value);
        }
        await db.SaveChangesAsync();
    }

    [Given(@"a patient named ""(.*)"" with owner phone ""(.*)""")]
    public async Task GivenAPatient(string name, string phone)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, name + "Owner", "Family", $"{name.ToLowerInvariant()}{Guid.NewGuid():N}@test.com", phone);
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);

        var patientResult = Patient.Create(clinicId, name, Species.Dog, "Mixed", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)));
        patientResult.IsSuccess.Should().BeTrue();
        var po = PatientOwner.Create(clinicId, patientResult.Value.Id, ownerResult.Value.Id);
        patientResult.Value.AddOwner(po);
        db.Patients.Add(patientResult.Value);
        await db.SaveChangesAsync();

        _lastPatientId = patientResult.Value.Id;
    }

    [Given(@"a patient named ""(.*)"" exists in clinic ""(.*)""")]
    public async Task GivenPatientInOtherClinic(string name, string clinicName)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds[clinicName];

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        var originalClinicId = testClinicContext.ClinicId;
        testClinicContext.ClinicId = clinicId;

        var ownerResult = Owner.Create(clinicId, "Other", "Owner", $"other@{clinicName.ToLowerInvariant().Replace(" ", "")}.ae", "+971 50 000 0001");
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);

        var patientResult = Patient.Create(clinicId, name, Species.Dog, "Mixed", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)));
        patientResult.IsSuccess.Should().BeTrue();
        var po = PatientOwner.Create(clinicId, patientResult.Value.Id, ownerResult.Value.Id);
        patientResult.Value.AddOwner(po);
        db.Patients.Add(patientResult.Value);
        await db.SaveChangesAsync();

        testClinicContext.ClinicId = originalClinicId;
    }

    [Given(@"2 medical records exist for ""(.*)""")]
    public async Task Given2MedicalRecords(string patientName)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        for (int i = 1; i <= 2; i++)
        {
            var recordResult = MedicalRecord.Create(clinicId, _lastPatientId,
                $"Diagnosis {i}", $"Treatment {i}", "Dr. Test", DateTime.UtcNow.AddDays(-i));
            recordResult.IsSuccess.Should().BeTrue();
            db.MedicalRecords.Add(recordResult.Value);
        }
        await db.SaveChangesAsync();
    }

    // ─── WHEN steps ──────────────────────────────────────────────

    [When(@"I create a patient with name ""(.*)"", species ""(.*)"", breed ""(.*)"", birth date ""(.*)"", owner name ""(.*)"", owner phone ""(.*)""")]
    public async Task WhenCreatePatient(string name, string species, string breed, string birthDate, string ownerName, string ownerPhone)
    {
        var request = new CreatePatientRequest(name, Enum.Parse<Species>(species), breed, DateOnly.Parse(birthDate), ownerName, ownerPhone);
        _response = await _client.PostAsJsonAsync("/api/v1/patients", request);

        if (_response.IsSuccessStatusCode)
            _patientDto = await _response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);
    }

    [When(@"I list patients with name filter ""(.*)""")]
    public async Task WhenListPatientsWithFilter(string nameFilter)
    {
        _response = await _client.GetAsync($"/api/v1/patients?name={Uri.EscapeDataString(nameFilter)}");
        if (_response.IsSuccessStatusCode)
        {
            var result = await _response.Content.ReadFromJsonAsync<PatientListResult>(JsonOptions);
            _patientList = result?.Items;
        }
    }

    [When(@"I update the patient owner phone to ""(.*)""")]
    public async Task WhenUpdatePatientPhone(string newPhone)
    {
        var request = new UpdatePatientRequest(null, null, null, null, null, newPhone);
        _response = await _client.PatchAsJsonAsync($"/api/v1/patients/{_lastPatientId}", request);
        if (_response.IsSuccessStatusCode)
            _patientDto = await _response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);
    }

    [When(@"I get the patient detail")]
    public async Task WhenGetPatientDetail()
    {
        _response = await _client.GetAsync($"/api/v1/patients/{_lastPatientId}/detail");
        if (_response.IsSuccessStatusCode)
            _patientDetailDto = await _response.Content.ReadFromJsonAsync<PatientDetailDto>(JsonOptions);
    }

    [When(@"I list patients as vet of clinic ""(.*)""")]
    public async Task WhenListPatientsAsVetOfClinic(string clinicName)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds[clinicName];

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        _response = await _client.GetAsync("/api/v1/patients");
        if (_response.IsSuccessStatusCode)
        {
            var result = await _response.Content.ReadFromJsonAsync<PatientListResult>(JsonOptions);
            _patientList = result?.Items;
        }
    }

    // ─── THEN steps ──────────────────────────────────────────────

    [Then(@"the patient is created successfully")]
    public void ThenPatientCreated()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _patientDto.Should().NotBeNull();
    }

    [Then(@"the patient name is ""(.*)""")]
    public void ThenPatientName(string expectedName)
    {
        _patientDto.Should().NotBeNull();
        _patientDto!.Name.Should().Be(expectedName);
    }

    [Then(@"the patient owner name is ""(.*)""")]
    public void ThenPatientOwnerName(string expectedOwnerName)
    {
        _patientDto.Should().NotBeNull();
        _patientDto!.OwnerName.Should().Be(expectedOwnerName);
    }

    [Then(@"the patient species is ""(.*)""")]
    public void ThenPatientSpecies(string expectedSpecies)
    {
        _patientDto.Should().NotBeNull();
        _patientDto!.Species.ToString().Should().Be(expectedSpecies);
    }

    [Then(@"I see 1 patient in the results")]
    public void ThenSeeOnePatient()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _patientList.Should().NotBeNull();
        _patientList!.Count.Should().Be(1);
    }

    [Then(@"the patient owner phone is ""(.*)""")]
    public void ThenPatientOwnerPhone(string expectedPhone)
    {
        _patientDto.Should().NotBeNull();
        _patientDto!.OwnerPhone.Should().Be(expectedPhone);
    }

    [Then(@"the request is rejected with status 403")]
    public void ThenRejectedWith403()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Then(@"I cannot see ""(.*)"" in the patient list")]
    public void ThenCannotSeePatient(string patientName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _patientList.Should().NotBeNull();
        _patientList!.Should().NotContain(p => p.Name == patientName);
    }

    [Then(@"the detail contains 2 medical records")]
    public void ThenDetailContains2Records()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _patientDetailDto.Should().NotBeNull();
        _patientDetailDto!.RecentRecords.Count.Should().Be(2);
    }
}

// Local helper record for paginated list deserialization
internal record PatientListResult(List<PatientDto> Items, int Total, int Page, int PageSize);
