using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

namespace Vetolib.Tests.Acceptance.StepDefinitions.Portal;

[Binding]
[Scope(Feature = "Owner Registration")]
internal class OwnerRegistrationSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage? _response;
    private OwnerAccountDto? _createdAccount;
    private OwnerPortalTokenDto? _portalToken;
    private string? _errorResponseBody;
    private string? _currentOwnerPassword;
    private string? _currentOwnerEmail;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public OwnerRegistrationSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // --- GIVEN Steps ---

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

    [Given(@"an owner ""(.*)"" with email ""(.*)"" exists at clinic ""(.*)"" with a cat named ""(.*)""")]
    public async Task GivenAnOwnerWithEmailExistsAtClinicWithCat(
        string ownerName, string email, string clinicName, string catName)
    {
        await SeedOwnerWithAnimal(ownerName, email, null, clinicName, catName, "Cat");
    }

    [Given(@"an owner ""(.*)"" with phone ""(.*)"" exists at clinic ""(.*)"" with a dog named ""(.*)""")]
    public async Task GivenAnOwnerWithPhoneExistsAtClinicWithDog(
        string ownerName, string phone, string clinicName, string dogName)
    {
        await SeedOwnerWithAnimal(ownerName, null, phone, clinicName, dogName, "Dog");
    }

    [Given(@"a portal account already exists with email ""(.*)""")]
    public async Task GivenAPortalAccountAlreadyExistsWithEmail(string email)
    {
        // Register a portal account so it occupies the email
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/portal/register", new
        {
            Email = email,
            Phone = $"+97150{new Random().Next(1000000, 9999999)}",
            FullName = "Existing User",
            Password = "SecurePass1!"
        });
        registerResponse.IsSuccessStatusCode.Should().BeTrue("Seeding existing account should succeed");
    }

    [Given(@"""(.*)"" has a portal account with email ""(.*)""")]
    public async Task GivenOwnerHasPortalAccountWithEmail(string ownerName, string email)
    {
        _currentOwnerEmail = email;
        _currentOwnerPassword = "SecurePass1!";

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/portal/register", new
        {
            Email = email,
            Phone = $"+97150{new Random().Next(1000000, 9999999)}",
            FullName = ownerName,
            Password = _currentOwnerPassword
        });
        registerResponse.IsSuccessStatusCode.Should().BeTrue(
            $"Creating portal account for {ownerName} should succeed");

        _createdAccount = await registerResponse.Content.ReadFromJsonAsync<OwnerAccountDto>(JsonOptions);
    }

    [Given(@"her account is linked to clinics ""(.*)"" and ""(.*)""")]
    public async Task GivenHerAccountIsLinkedToClinics(string clinic1, string clinic2)
    {
        _createdAccount.Should().NotBeNull("Portal account must exist before linking clinics");

        // Seed owners at both clinics with the same email, then the auto-linker will link them
        // The register endpoint already auto-links by email. We need to seed Owner records
        // in MedicalRecords at the two clinics with matching email.
        var clinicId1 = SharedSteps.GenerateGuidFromString(clinic1);
        var clinicId2 = SharedSteps.GenerateGuidFromString(clinic2);

        using var scope = _factory.Services.CreateScope();
        var medicalDb = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        // Seed owner at clinic1 and link to account
        var owner1Result = Owner.Create(clinicId1, "Fatima", "Al Rashid", _currentOwnerEmail!, null);
        owner1Result.IsSuccess.Should().BeTrue();
        owner1Result.Value.LinkOwnerAccount(_createdAccount!.Id);
        medicalDb.Owners.Add(owner1Result.Value);

        // Seed owner at clinic2 and link to account
        var owner2Result = Owner.Create(clinicId2, "Fatima", "Al Rashid", _currentOwnerEmail!, null);
        owner2Result.IsSuccess.Should().BeTrue();
        owner2Result.Value.LinkOwnerAccount(_createdAccount!.Id);
        medicalDb.Owners.Add(owner2Result.Value);

        await medicalDb.SaveChangesAsync();
    }

    [Given(@"a patient with microchip ""(.*)"" exists at clinic ""(.*)"" owned by an unlinked owner")]
    public async Task GivenAPatientWithMicrochipExistsAtClinic(string microchip, string clinicName)
    {
        var clinicId = SharedSteps.GenerateGuidFromString(clinicName);

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, "Unlinked", "Owner", "unlinked@seed.com", null);
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);

        var patientResult = Patient.Create(clinicId, "MicrochipPet", Species.Dog, "Mixed",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)), microchipNumber: microchip);
        patientResult.IsSuccess.Should().BeTrue();
        var patient = patientResult.Value;
        var po = PatientOwner.Create(clinicId, patient.Id, ownerResult.Value.Id);
        patient.AddOwner(po);
        db.Patients.Add(patient);

        await db.SaveChangesAsync();

        _ctx.Set(ownerResult.Value.Id, "UnlinkedOwnerId");

        // Reset test clinic context
        testClinicContext.ClinicId = TestClinicContext.TestClinicGuid;
    }

    // --- WHEN Steps ---

    [When(@"""(.*)"" registers on the portal with email ""(.*)"" and phone ""(.*)""")]
    public async Task WhenOwnerRegistersOnThePortal(string ownerName, string email, string phone)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/register", new
        {
            FullName = ownerName,
            Email = email,
            Phone = phone,
            Password = "SecurePass1!"
        });

        if (_response.IsSuccessStatusCode)
        {
            _createdAccount = await _response.Content.ReadFromJsonAsync<OwnerAccountDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"someone tries to register with email ""(.*)""")]
    public async Task WhenSomeoneTriesToRegisterWithEmail(string email)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/register", new
        {
            FullName = "Someone New",
            Email = email,
            Phone = "+971500000000",
            Password = "SecurePass1!"
        });

        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she logs in with her credentials")]
    public async Task WhenSheLogsInWithHerCredentials()
    {
        _currentOwnerEmail.Should().NotBeNullOrEmpty("Owner email must be set before login");
        _currentOwnerPassword.Should().NotBeNullOrEmpty("Owner password must be set before login");

        _response = await _client.PostAsJsonAsync("/api/v1/portal/login", new
        {
            Email = _currentOwnerEmail,
            Password = _currentOwnerPassword
        });

        if (_response.IsSuccessStatusCode)
        {
            _portalToken = await _response.Content.ReadFromJsonAsync<OwnerPortalTokenDto>(JsonOptions);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _portalToken!.AccessToken);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she provides microchip number ""(.*)""")]
    public async Task WhenSheProvidesChipNumber(string microchip)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/link-microchip", new
        {
            MicrochipNumber = microchip
        });

        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    // --- THEN Steps ---

    [Then(@"her account is created successfully")]
    public void ThenHerAccountIsCreatedSuccessfully()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdAccount.Should().NotBeNull();
    }

    [Then(@"his account is created successfully")]
    public void ThenHisAccountIsCreatedSuccessfully()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdAccount.Should().NotBeNull();
    }

    [Then(@"her account is automatically linked to the owner record at ""(.*)""")]
    public async Task ThenHerAccountIsLinkedToOwnerRecordAt(string clinicName)
    {
        _createdAccount.Should().NotBeNull();

        // Verify by checking the Owner record in MedicalRecords DB has OwnerAccountId set
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var linkedOwner = await db.Owners
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.OwnerAccountId == _createdAccount!.Id);

        linkedOwner.Should().NotBeNull($"Owner should be linked to account at {clinicName}");
    }

    [Then(@"his account is automatically linked to the owner record at ""(.*)""")]
    public async Task ThenHisAccountIsLinkedToOwnerRecordAt(string clinicName)
    {
        _createdAccount.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var linkedOwner = await db.Owners
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.OwnerAccountId == _createdAccount!.Id);

        linkedOwner.Should().NotBeNull($"Owner should be linked to account at {clinicName}");
    }

    [Then(@"the registration is rejected because the email is already taken")]
    public void ThenRegistrationIsRejectedBecauseEmailIsTaken()
    {
        _response.Should().NotBeNull();
        _response!.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNullOrEmpty();
    }

    [Then(@"she receives a token containing her linked clinic identifiers")]
    public void ThenSheReceivesATokenWithLinkedClinicIds()
    {
        _portalToken.Should().NotBeNull();
        _portalToken!.LinkedClinicIds.Should().NotBeEmpty();
    }

    [Then(@"the patient's owner is linked to her account")]
    public async Task ThenThePatientOwnerIsLinkedToHerAccount()
    {
        _response.Should().NotBeNull();
        _response!.IsSuccessStatusCode.Should().BeTrue(
            $"Link by microchip should succeed but got: {_errorResponseBody}");

        // Verify the owner record was linked
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var unlinkedOwnerId = _ctx.Get<Guid>("UnlinkedOwnerId");
        var owner = await db.Owners.IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == unlinkedOwnerId);

        owner.Should().NotBeNull();
        owner!.OwnerAccountId.Should().NotBeNull("Owner should now be linked to the portal account");
    }

    // --- Helpers ---

    private async Task SeedOwnerWithAnimal(
        string ownerName, string? email, string? phone,
        string clinicName, string animalName, string species)
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

        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : "";

        var ownerResult = Owner.Create(clinicId, firstName, lastName, email ?? $"{firstName.ToLowerInvariant()}@seed.com", phone);
        ownerResult.IsSuccess.Should().BeTrue($"Owner creation should succeed for {ownerName}");

        db.Owners.Add(ownerResult.Value);
        await db.SaveChangesAsync();

        _ctx.Set(ownerResult.Value.Id, $"OwnerId_{ownerName}");
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
