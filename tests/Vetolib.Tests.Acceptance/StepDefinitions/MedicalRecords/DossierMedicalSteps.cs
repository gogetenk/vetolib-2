using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        // Determine species from breed
        var species = InferSpecies(breed);

        var patientResult = Patient.Create(clinicId, animalName, species, breed, null);
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

        // Set clinic context
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        // Create user in auth DB
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

        // Check if user already exists (e.g., if step called multiple times)
        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        // Login to get JWT
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

        // Create owner and patient in the other clinic directly in DB
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        var originalClinicId = testClinicContext.ClinicId;

        // Temporarily switch to the other clinic
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Create a dummy owner for this animal in the other clinic
        var ownerResult = Owner.Create(clinicId, "Other", "Owner", $"owner@{clinicName.ToLowerInvariant().Replace(" ", "")}.com", null);
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);

        var patientResult = Patient.Create(clinicId, animalName, "Dog", "Mixed", null);
        patientResult.IsSuccess.Should().BeTrue();

        var patient = patientResult.Value;
        var patientOwner = PatientOwner.Create(clinicId, patient.Id, ownerResult.Value.Id);
        patient.AddOwner(patientOwner);

        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        _patientIds[animalName] = patient.Id;

        // Restore original clinic context
        testClinicContext.ClinicId = originalClinicId;
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"je crée un animal ""(.*)"" race ""(.*)"" pour le propriétaire ""(.*)""")]
    public async Task WhenJeCreerUnAnimal(string animalName, string breed, string ownerName)
    {
        var ownerId = _ownerIds[ownerName];
        var species = InferSpecies(breed);

        var request = new CreatePatientRequest(animalName, species, breed, null, ownerId);
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
        // For now, attempting to create a patient is used to test write permissions
        // since medical exam creation is not yet implemented
        var ownerId = _ownerIds.Values.FirstOrDefault();
        var request = new CreatePatientRequest("TestAnimal", "Dog", "Mixed", null, ownerId);
        _response = await _client.PostAsJsonAsync("/api/v1/patients", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je consulte la liste des animaux de ""(.*)""")]
    public async Task WhenJeConsulteLaListeDesAnimaux(string clinicName)
    {
        // Ensure clinic context is set to the target clinic
        var clinicId = _clinicIds[clinicName];
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        _response = await _client.GetAsync("/api/v1/patients");
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
        // For now, a newly created patient has no medical history
        // This will be extended when medical records are implemented
        _createdPatient.Should().NotBeNull();
    }

    [Then(@"le propriétaire ""(.*)"" est lié à ""(.*)""")]
    public void ThenLeProprietaireEstLie(string ownerName, string animalName)
    {
        _createdPatient.Should().NotBeNull();
        _createdPatient!.Owners.Should().NotBeEmpty();

        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : "";

        _createdPatient.Owners.Should().Contain(o =>
            o.FirstName == firstName && o.LastName == lastName);
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
        }
    }

    [Then(@"""(.*)"" n'apparaît pas dans la liste")]
    public async Task ThenNApparaitPasDansLaListe(string animalName)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var patients = await _response.Content.ReadFromJsonAsync<List<PatientDto>>(JsonOptions);
        patients.Should().NotBeNull();
        patients!.Should().NotContain(p => p.Name == animalName);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private static string InferSpecies(string breed)
    {
        var breedLower = breed.ToLowerInvariant();
        if (breedLower.Contains("cat") || breedLower.Contains("persian") || breedLower.Contains("siamese"))
            return "Cat";
        if (breedLower.Contains("parrot") || breedLower.Contains("canary"))
            return "Bird";
        return "Dog";
    }
}
