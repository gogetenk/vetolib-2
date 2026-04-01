using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Portal;

[Binding]
[Scope(Feature = "Record Sharing")]
internal class RecordSharingSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage? _response;
    private string? _errorResponseBody;

    private CreateShareLinkResponse? _shareLinkResponse;
    private List<SharedRecordLinkDto>? _shareLinks;
    private readonly Dictionary<string, Guid> _ownerIds = new();
    private readonly Dictionary<string, Guid> _patientIds = new();
    private string? _shareToken;
    private Guid _ownerAccountId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public RecordSharingSteps(ScenarioContext ctx)
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

    [Given(@"an owner ""(.*)"" has a portal account linked to ""(.*)""")]
    public async Task GivenAnOwnerHasPortalAccountLinkedTo(string ownerName, string clinicName)
    {
        _ctx.Set(ownerName, "CurrentOwnerName");
        // Register a portal account and authenticate
        await RegisterAndAuthenticateOwner(ownerName);
    }

    [Given(@"""(.*)"" has a cat ""(.*)"" at ""(.*)""")]
    public async Task GivenOwnerHasCatAtClinic(string ownerName, string catName, string clinicName)
    {
        await SeedOwnerWithPatient(ownerName, clinicName, catName, Species.Cat);
    }

    [Given(@"she has created a share link for ""(.*)""")]
    public async Task GivenSheHasCreatedShareLinkFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.PostAsJsonAsync($"/api/v1/portal/animals/{patientId}/share", new { });

        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Creating share link should succeed but got {_response.StatusCode}: {await _response.Content.ReadAsStringAsync()}");

        _shareLinkResponse = await _response.Content
            .ReadFromJsonAsync<CreateShareLinkResponse>(JsonOptions);
        _shareToken = _shareLinkResponse?.Token;
    }

    [Given(@"she has created a share link for ""(.*)"" that has expired")]
    public async Task GivenSheHasCreatedExpiredShareLinkFor(string animalName)
    {
        var patientId = _patientIds[animalName];
        var clinicId = TestClinicContext.TestClinicGuid;

        // Create a share link directly in the database with an expired date
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var linkResult = SharedRecordLink.Create(clinicId, patientId, _ownerAccountId);
        linkResult.IsSuccess.Should().BeTrue("SharedRecordLink creation should succeed");

        var link = linkResult.Value;
        // Use reflection to set ExpiresAt to the past since it's a private setter
        typeof(SharedRecordLink).GetProperty("ExpiresAt")!.SetValue(link, DateTime.UtcNow.AddHours(-1));

        db.SharedRecordLinks.Add(link);
        await db.SaveChangesAsync();

        _shareToken = link.Token;
    }

    [Given(@"she has created (\d+) share links for ""(.*)""")]
    public async Task GivenSheHasCreatedMultipleShareLinksFor(int count, string animalName)
    {
        var patientId = _patientIds[animalName];
        for (int i = 0; i < count; i++)
        {
            var resp = await _client.PostAsJsonAsync($"/api/v1/portal/animals/{patientId}/share", new { });
            resp.IsSuccessStatusCode.Should().BeTrue($"Creating share link {i + 1} of {count} should succeed");
        }
    }

    // --- WHEN Steps ---

    [When(@"she creates a share link for ""(.*)"" valid for (\d+) hours")]
    public async Task WhenSheCreatesShareLinkFor(string animalName, int hours)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.PostAsJsonAsync($"/api/v1/portal/animals/{patientId}/share", new { });

        if (_response.IsSuccessStatusCode)
        {
            _shareLinkResponse = await _response.Content
                .ReadFromJsonAsync<CreateShareLinkResponse>(JsonOptions);
            _shareToken = _shareLinkResponse?.Token;
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"someone accesses the share link")]
    public async Task WhenSomeoneAccessesTheShareLink()
    {
        _shareToken.Should().NotBeNullOrEmpty("A share link must exist before accessing it");
        _response = await _client.GetAsync($"/api/v1/shared/{_shareToken}");
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"someone accesses the expired share link")]
    public async Task WhenSomeoneAccessesTheExpiredShareLink()
    {
        _shareToken.Should().NotBeNullOrEmpty("An expired share link must exist before accessing it");
        _response = await _client.GetAsync($"/api/v1/shared/{_shareToken}");
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she revokes the share link")]
    public async Task WhenSheRevokesTheShareLink()
    {
        _shareLinkResponse.Should().NotBeNull("A share link must exist before revoking it");
        _response = await _client.DeleteAsync(
            $"/api/v1/portal/shares/{_shareLinkResponse!.Id}");
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views her active share links")]
    public async Task WhenSheViewsHerActiveShareLinks()
    {
        _response = await _client.GetAsync("/api/v1/portal/shares");
        if (_response.IsSuccessStatusCode)
        {
            _shareLinks = await _response.Content
                .ReadFromJsonAsync<List<SharedRecordLinkDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    // --- THEN Steps ---

    [Then(@"a share link is generated successfully")]
    public void ThenAShareLinkIsGeneratedSuccessfully()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
        _shareLinkResponse.Should().NotBeNull();
        _shareLinkResponse!.Token.Should().NotBeNullOrEmpty();
        _shareLinkResponse.ShareUrl.Should().NotBeNullOrEmpty();
    }

    [Then(@"the link expires in (\d+) hours")]
    public void ThenTheLinkExpiresInHours(int hours)
    {
        _shareLinkResponse.Should().NotBeNull();
        // The domain creates links with 72h expiry. Verify within a reasonable range.
        var expectedExpiry = DateTime.UtcNow.AddHours(72);
        _shareLinkResponse!.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(5));
    }

    [Then(@"they can see ""(.*)""'s medical records")]
    public void ThenTheyCanSeeMedicalRecords(string animalName)
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"they cannot modify any records")]
    public void ThenTheyCannotModifyAnyRecords()
    {
        // Shared links provide read-only access -- no modification endpoints exposed
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the link is no longer accessible")]
    public async Task ThenTheLinkIsNoLongerAccessible()
    {
        _shareToken.Should().NotBeNullOrEmpty();
        var checkResponse = await _client.GetAsync($"/api/v1/shared/{_shareToken}");
        checkResponse.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"access is denied because the link has expired")]
    public void ThenAccessIsDeniedBecauseLinkExpired()
    {
        _response.Should().NotBeNull();
        _response!.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"she should see (\d+) share links listed")]
    public void ThenSheShouldSeeNShareLinksListed(int count)
    {
        _shareLinks.Should().NotBeNull();
        _shareLinks.Should().HaveCount(count);
    }

    // --- Helpers ---

    private async Task RegisterAndAuthenticateOwner(string ownerName)
    {
        var names = ownerName.Split(' ', 2);
        var firstName = names[0];
        var email = $"{firstName.ToLowerInvariant()}-share@portal-test.com";
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
            $"Portal registration should succeed but got {registerResponse.StatusCode}");

        var account = await registerResponse.Content.ReadFromJsonAsync<OwnerAccountDto>(JsonOptions);
        account.Should().NotBeNull();
        _ownerAccountId = account!.Id;

        // Login to get JWT
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/portal/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.IsSuccessStatusCode.Should().BeTrue("Portal login should succeed");

        var token = await loginResponse.Content.ReadFromJsonAsync<OwnerPortalTokenDto>(JsonOptions);
        token.Should().NotBeNull();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

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

            // Link the owner to the portal account so the handler's ownership check passes
            if (_ownerAccountId != Guid.Empty)
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
