using System.Net;
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
    private OwnerPortalTokenDto? _portalToken = null;
    private string? _errorResponseBody;

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
    public void GivenAPortalAccountAlreadyExistsWithEmail(string email)
    {
        // Seed a portal account in the auth DB
        throw new PendingStepException();
    }

    [Given(@"""(.*)"" has a portal account with email ""(.*)""")]
    public void GivenOwnerHasPortalAccountWithEmail(string ownerName, string email)
    {
        // Seed portal account for given owner
        throw new PendingStepException();
    }

    [Given(@"her account is linked to clinics ""(.*)"" and ""(.*)""")]
    public void GivenHerAccountIsLinkedToClinics(string clinic1, string clinic2)
    {
        // Link account to multiple clinics
        throw new PendingStepException();
    }

    [Given(@"a patient with microchip ""(.*)"" exists at clinic ""(.*)"" owned by an unlinked owner")]
    public void GivenAPatientWithMicrochipExistsAtClinic(string microchip, string clinicName)
    {
        // Seed a patient with microchip at the given clinic
        throw new PendingStepException();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

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
        // Login via portal login endpoint
        throw new PendingStepException();
    }

    [When(@"she provides microchip number ""(.*)""")]
    public async Task WhenSheProvidesChipNumber(string microchip)
    {
        // Call the microchip-based linking endpoint
        throw new PendingStepException();
    }

    // ─── THEN Steps ──────────────────────────────────────────────

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
    public void ThenHerAccountIsLinkedToOwnerRecordAt(string clinicName)
    {
        // Verify the account was linked to the clinic's existing owner record
        throw new PendingStepException();
    }

    [Then(@"his account is automatically linked to the owner record at ""(.*)""")]
    public void ThenHisAccountIsLinkedToOwnerRecordAt(string clinicName)
    {
        // Verify the account was linked to the clinic's existing owner record
        throw new PendingStepException();
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
    public void ThenThePatientOwnerIsLinkedToHerAccount()
    {
        // Verify the owner record was linked via microchip
        throw new PendingStepException();
    }

    // ─── Helpers ─────────────────────────────────────────────────

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
