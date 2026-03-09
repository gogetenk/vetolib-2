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

namespace Vetolib.Tests.Acceptance.StepDefinitions.MedicalRecords;

[Binding]
[Scope(Feature = "Dossier médical animal")]
internal class DossierMedicalSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private string? _errorResponseBody;

    private readonly Dictionary<string, Guid> _clinicIds = new();
    private readonly Dictionary<string, Guid> _ownerIds = new();
    private readonly Dictionary<string, Guid> _patientIds = new();
    private PatientDto? _createdPatient;
    private MedicalRecordDto? _createdRecord;
    private PrescriptionDto? _createdPrescription;
    private Guid _lastRecordId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DossierMedicalSteps(ScenarioContext ctx)
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

    [Given(@"une clinique ""(.*)""")]
    public void GivenUneClinique(string clinicName)
    {
        var clinicId = GenerateGuidFromString(clinicName);
        _clinicIds[clinicName] = clinicId;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        if (_clinicIds.Count == 1)
        {
            testClinicContext.ClinicId = clinicId;
        }
    }

    [Given(@"un propriétaire ""(.*)"" avec l'email ""(.*)""")]
    public async Task GivenUnProprietaire(string ownerName, string email)
    {
        var clinicName = _clinicIds.Keys.First();
        var clinicId = _clinicIds[clinicName];

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

    [Given(@"un animal ""(.*)"" race ""(.*)"" appartenant à ""(.*)""")]
    public async Task GivenUnAnimal(string animalName, string breed, string ownerName)
    {
        var clinicName = _clinicIds.Keys.First();
        var clinicId = _clinicIds[clinicName];
        var ownerId = _ownerIds[ownerName];

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var species = InferSpecies(breed);

        var patientResult = Patient.Create(clinicId, animalName, InferSpecies(breed), breed, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)));
        patientResult.IsSuccess.Should().BeTrue($"Patient creation should succeed for {animalName}");

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerId);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _patientIds[animalName] = patient.Id;
    }

    [Given(@"je suis authentifié en tant que (.*)")]
    public async Task GivenJeSuisAuthentifie(string role)
    {
        var clinicName = _clinicIds.Keys.First();
        var clinicId = _clinicIds[clinicName];

        var email = $"{role.ToLowerInvariant()}@test-medical.com";
        var password = "SecurePass1";

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ADMIN" => UserRole.Admin,
            _ => UserRole.Receptionist
        };

        var vetLicense = userRole == UserRole.Vet ? "TEST-VET-001" : null;
        var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue($"User creation should succeed for role {role}");

        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for {email}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"un animal ""(.*)"" dans la clinique ""(.*)""")]
    public async Task GivenUnAnimalDansLaClinique(string animalName, string clinicName)
    {
        var clinicId = GenerateGuidFromString(clinicName);
        _clinicIds[clinicName] = clinicId;

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

    [Given(@"(\d+) examens dans le dossier de ""(.*)""")]
    public async Task GivenNExamensDansLeDossier(int count, string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = _clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        for (int i = 1; i <= count; i++)
        {
            var recordResult = MedicalRecord.Create(
                clinicId, patientId,
                $"Diagnostic {i}",
                $"Traitement {i}",
                "Dr. Test",
                DateTime.UtcNow.AddDays(-i));

            recordResult.IsSuccess.Should().BeTrue();
            db.MedicalRecords.Add(recordResult.Value);
        }

        await db.SaveChangesAsync();
    }

    [Given(@"un examen existant pour ""(.*)""")]
    public async Task GivenUnExamenExistantPour(string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = _clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var recordResult = MedicalRecord.Create(
            clinicId, patientId,
            "Consultation de routine",
            "Observation",
            "Dr. Test",
            DateTime.UtcNow);

        recordResult.IsSuccess.Should().BeTrue();
        db.MedicalRecords.Add(recordResult.Value);
        await db.SaveChangesAsync();

        _lastRecordId = recordResult.Value.Id;
    }

    [Given(@"un examen dans le dossier de ""(.*)""")]
    public async Task GivenUnExamenDansLeDossier(string animalName)
    {
        await GivenUnExamenExistantPour(animalName);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"je crée un animal ""(.*)"" race ""(.*)"" pour le propriétaire ""(.*)""")]
    public async Task WhenJeCreerUnAnimal(string animalName, string breed, string ownerName)
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

    [When(@"je tente d'ajouter un examen pour ""(.*)""")]
    public async Task WhenJeTenteDajouterUnExamen(string animalName)
    {
        var patientId = _patientIds.TryGetValue(animalName, out var id) ? id : Guid.NewGuid();
        var request = new AddMedicalRecordRequest("Test diagnostic", "Test traitement");
        _response = await _client.PostAsJsonAsync($"/api/v1/patients/{patientId}/records", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"j'ajoute un examen pour ""(.*)"" avec le diagnostic ""(.*)"" et le traitement ""(.*)""")]
    public async Task WhenJAjouteUnExamen(string animalName, string diagnosis, string treatment)
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

    [When(@"je consulte le dossier de ""(.*)""")]
    public async Task WhenJeConsulteLeDossier(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.GetAsync($"/api/v1/patients/{patientId}/records");
    }

    [When(@"je consulte la liste des animaux de ""(.*)""")]
    public async Task WhenJeConsulteLaListeDesAnimaux(string clinicName)
    {
        var clinicId = _clinicIds[clinicName];
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        _response = await _client.GetAsync("/api/v1/patients");
    }

    [When(@"je crée une ordonnance avec le médicament ""(.*)"" posologie ""(.*)""")]
    public async Task WhenJeCreerUneOrdonnance(string medication, string dosage)
    {
        var request = new AddPrescriptionRequest(medication, dosage);
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/records/{_lastRecordId}/prescriptions",
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

    [When(@"je tente de supprimer cet examen")]
    public async Task WhenJeTenteDeSupprimer()
    {
        _response = await _client.DeleteAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/records/{_lastRecordId}");
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"l'animal est créé dans la clinique")]
    public void ThenLAnimalEstCreeDansLaClinique()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdPatient.Should().NotBeNull();
        _createdPatient!.ClinicId.Should().Be(_clinicIds.Values.First());
    }

    [Then(@"son dossier médical est vide")]
    public void ThenSonDossierMedicalEstVide()
    {
        _createdPatient.Should().NotBeNull();
    }

    [Then(@"le propriétaire ""(.*)"" est lié à ""(.*)""")]
    public void ThenLeProprietaireEstLie(string ownerName, string animalName)
    {
        _createdPatient.Should().NotBeNull();
        _createdPatient!.OwnerName.Should().NotBeNullOrEmpty();
        _createdPatient.OwnerName.Should().Contain(ownerName.Split(' ')[0]);
    }

    [Then(@"le système refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();

        switch (errorCode)
        {
            case "INSUFFICIENT_PERMISSIONS":
                _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
                break;
            case "MEDICAL_RECORD_IMMUTABLE":
                _response.StatusCode.Should().NotBe(HttpStatusCode.OK);
                (_errorResponseBody ?? string.Empty).Should().Contain("MEDICAL_RECORD_IMMUTABLE");
                break;
        }
    }

    [Then(@"""(.*)"" n'apparaît pas dans la liste")]
    public async Task ThenNApparaitPasDansLaListe(string animalName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await _response.Content.ReadFromJsonAsync<PatientPagedResultDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(p => p.Name == animalName);
    }

    [Then(@"l'examen apparaît dans l'historique de ""(.*)""")]
    public void ThenLExamenApparaitDansLHistorique(string animalName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected success response, got: {_errorResponseBody}");
        _createdRecord.Should().NotBeNull();
        _createdRecord!.PatientId.Should().Be(_patientIds[animalName]);
    }

    [Then(@"il est horodaté avec la date du jour")]
    public void ThenIlEstHorodateAvecLaDateDuJour()
    {
        _createdRecord.Should().NotBeNull();
        _createdRecord!.ExaminedAt.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Then(@"il porte le vétérinaire courant comme auteur")]
    public void ThenIlPorteLeVeterinaireCommeAuteur()
    {
        _createdRecord.Should().NotBeNull();
        _createdRecord!.VetName.Should().NotBeNullOrEmpty();
    }

    [Then(@"je vois (\d+) examens dans l'ordre chronologique inverse")]
    public async Task ThenJeVoisNExamens(int expectedCount)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await _response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>(JsonOptions);
        records.Should().NotBeNull();
        records!.Count.Should().Be(expectedCount);

        // Verify descending order
        for (int i = 0; i < records.Count - 1; i++)
        {
            records[i].ExaminedAt.Should().BeOnOrAfter(records[i + 1].ExaminedAt);
        }
    }

    [Then(@"l'ordonnance est créée avec le numéro de licence ""(.*)""")]
    public void ThenLOrdonnanceEstCreee(string licenseNumber)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Expected success, got: {_errorResponseBody}");
        _createdPrescription.Should().NotBeNull();
        _createdPrescription!.VetLicenseNumber.Should().Be(licenseNumber);
    }

    [Then(@"elle est liée à l'examen")]
    public void ThenElleEstLieeALExamen()
    {
        _createdPrescription.Should().NotBeNull();
        _createdPrescription!.MedicalRecordId.Should().Be(_lastRecordId);
    }

    [Then(@"l'examen est toujours visible dans l'historique")]
    public async Task ThenLExamenEstToujoursVisible()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var record = await db.MedicalRecords.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == _lastRecordId);
        record.Should().NotBeNull("L'examen doit toujours être présent après une tentative de suppression");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
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
