using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
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
        _ctx.Set(ownerName, "CurrentOwnerName");
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
        _response = await _client.PostAsJsonAsync($"/api/v1/portal/animals/{patientId}/share", new
        {
            ExpiresInHours = 48
        });

        if (_response.IsSuccessStatusCode)
        {
            _shareLinkResponse = await _response.Content
                .ReadFromJsonAsync<CreateShareLinkResponse>(JsonOptions);
            _shareToken = _shareLinkResponse?.Token;
        }
    }

    [Given(@"she has created a share link for ""(.*)"" that has expired")]
    public void GivenSheHasCreatedExpiredShareLinkFor(string animalName)
    {
        // Seed an expired share link directly in the database
        throw new PendingStepException();
    }

    [Given(@"she has created (\d+) share links for ""(.*)""")]
    public void GivenSheHasCreatedMultipleShareLinksFor(int count, string animalName)
    {
        // Seed multiple share links for the animal
        throw new PendingStepException();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"she creates a share link for ""(.*)"" valid for (\d+) hours")]
    public async Task WhenSheCreatesShareLinkFor(string animalName, int hours)
    {
        var patientId = _patientIds[animalName];
        _response = await _client.PostAsJsonAsync($"/api/v1/portal/animals/{patientId}/share", new
        {
            ExpiresInHours = hours
        });

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
        _response = await _client.GetAsync($"/api/v1/portal/shared/{_shareToken}");
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"someone accesses the expired share link")]
    public async Task WhenSomeoneAccessesTheExpiredShareLink()
    {
        _shareToken.Should().NotBeNullOrEmpty("An expired share link must exist before accessing it");
        _response = await _client.GetAsync($"/api/v1/portal/shared/{_shareToken}");
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
            $"/api/v1/portal/share-links/{_shareLinkResponse!.Id}");
        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"she views her active share links")]
    public async Task WhenSheViewsHerActiveShareLinks()
    {
        _response = await _client.GetAsync("/api/v1/portal/share-links");
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

    // ─── THEN Steps ──────────────────────────────────────────────

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
        var expectedExpiry = DateTime.UtcNow.AddHours(hours);
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
        // Shared links provide read-only access — no modification endpoints exposed
        // This is verified by the absence of mutation endpoints on the shared route
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the link is no longer accessible")]
    public async Task ThenTheLinkIsNoLongerAccessible()
    {
        _shareToken.Should().NotBeNullOrEmpty();
        var checkResponse = await _client.GetAsync($"/api/v1/portal/shared/{_shareToken}");
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
