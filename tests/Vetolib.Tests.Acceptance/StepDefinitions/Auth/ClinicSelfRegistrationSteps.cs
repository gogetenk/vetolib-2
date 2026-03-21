using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "Clinic self-service registration")]
internal class ClinicSelfRegistrationSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private string? _responseBody;
    private RegisterClinicResponse? _registrationResponse;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ClinicSelfRegistrationSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN ────────────────────────────────────────────────────

    [Given(@"a clinic ""(.*)"" already registered with email ""(.*)""")]
    public async Task GivenAClinicAlreadyRegistered(string clinicName, string email)
    {
        var request = new RegisterClinicRequest(clinicName, email, "Secure@1234567!", "+971501234567", "AE");
        var response = await _client.PostAsJsonAsync("/api/v1/clinics/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Pre-seeding clinic '{clinicName}' should succeed");
    }

    // ─── WHEN ─────────────────────────────────────────────────────

    [When(@"^I register a new clinic with:$")]
    public async Task WhenIRegisterANewClinicWith(DataTable table)
    {
        var row = table.Rows[0];
        var request = new RegisterClinicRequest(
            row["ClinicName"],
            row["Email"],
            row["Password"],
            row["Phone"],
            row["Country"]);

        _response = await _client.PostAsJsonAsync("/api/v1/clinics/register", request);
        _responseBody = await _response.Content.ReadAsStringAsync();

        if (_response.IsSuccessStatusCode)
        {
            _registrationResponse = System.Text.Json.JsonSerializer.Deserialize<RegisterClinicResponse>(
                _responseBody, JsonOptions);
        }
    }

    // ─── THEN ─────────────────────────────────────────────────────

    [Then(@"the record is created successfully")]
    public void ThenTheRecordIsCreatedSuccessfully()
    {
        ((int)_response.StatusCode).Should().Be(201);
    }

    [Then(@"the operation is rejected with validation errors")]
    public void ThenTheOperationIsRejectedWithValidationErrors()
    {
        ((int)_response.StatusCode).Should().Be(422);
    }

    [Then(@"the request is rejected")]
    public void ThenTheRequestIsRejected()
    {
        ((int)_response.StatusCode).Should().Be(400);
    }

    [Then(@"I am successfully authenticated")]
    public void ThenIAmSuccessfullyAuthenticated()
    {
        _registrationResponse.Should().NotBeNull();
        _registrationResponse!.AccessToken.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(_registrationResponse.AccessToken).Should().BeTrue();
    }

    [Then(@"the session is linked to the clinic")]
    public void ThenTheSessionIsLinkedToTheClinic()
    {
        _registrationResponse.Should().NotBeNull();
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_registrationResponse!.AccessToken);
        token.Claims.Should().Contain(c => c.Type == "clinic_id",
            "JWT should contain claim 'clinic_id'");
    }

    [Then(@"the clinic ""(.*)"" is created")]
    public async Task ThenTheClinicIsCreated(string clinicName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var clinic = await db.Clinics
            .FirstOrDefaultAsync(c => c.Name == clinicName);

        clinic.Should().NotBeNull($"Clinic '{clinicName}' should exist in the database");
    }

    [Then(@"the admin user ""(.*)"" belongs to the new clinic")]
    public async Task ThenTheAdminUserBelongsToNewClinic(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        user.Should().NotBeNull($"User '{email}' should exist");
        user!.Role.Should().Be(UserRole.Admin, "the first user of a new clinic is Admin");

        var clinic = await db.Clinics
            .FirstOrDefaultAsync(c => c.Id == user.ClinicId);

        clinic.Should().NotBeNull("User should belong to an existing clinic");
    }

    [Then(@"the clinic trial ends in 14 days from now")]
    public async Task ThenTheClinicTrialEndsIn14Days()
    {
        _registrationResponse.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_registrationResponse!.AccessToken);
        var clinicIdClaim = token.Claims.First(c => c.Type == "clinic_id");
        var clinicId = Guid.Parse(clinicIdClaim.Value);

        var clinic = await db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId);
        clinic.Should().NotBeNull();

        var expectedTrialEnd = DateTime.UtcNow.AddDays(14);
        clinic!.TrialEndsAt.Should().BeCloseTo(expectedTrialEnd, TimeSpan.FromMinutes(1),
            "Trial should end in 14 days");
    }

    [Then(@"the clinic subscription plan is ""(.*)""")]
    public async Task ThenTheClinicSubscriptionPlanIs(string plan)
    {
        _registrationResponse.Should().NotBeNull();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_registrationResponse!.AccessToken);
        var clinicIdClaim = token.Claims.First(c => c.Type == "clinic_id");
        var clinicId = Guid.Parse(clinicIdClaim.Value);

        var clinic = await db.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId);
        clinic.Should().NotBeNull();
        clinic!.SubscriptionPlan.Should().Be(plan);
    }

    [Then(@"the response contains ""(.*)""")]
    public void ThenTheResponseContains(string expectedText)
    {
        _responseBody.Should().Contain(expectedText);
    }

    [Then(@"the operation is rejected because (.*) is invalid")]
    public void ThenTheOperationIsRejectedBecauseFieldIsInvalid(string fieldName)
    {
        _responseBody.Should().NotBeNull();
        _responseBody!.ToLower().Should().Contain(fieldName.ToLower(),
            $"Response should contain validation error for field '{fieldName}'");
    }
}
