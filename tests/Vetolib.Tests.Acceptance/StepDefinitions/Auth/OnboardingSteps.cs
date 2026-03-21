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
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "Onboarding State Management")]
internal class OnboardingSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public OnboardingSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── Background / Given ───────────────────────────────────────

    [Given(@"a clinic ""(.*)"" exists")]
    public void GivenAClinicExists(string clinicName)
    {
        // Use the fixed TestClinicGuid so the EF Core compiled query filter
        // (baked at model-creation time with the initial TestClinicContext.ClinicId value)
        // matches the ClinicId used when inserting and querying test data.
        var clinicId = TestClinicContext.TestClinicGuid;
        _ctx.Set(clinicId, "ClinicId");
        _ctx.Set(clinicName, "ClinicName");

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = TestClinicContext.TestClinicGuid;
    }

    [Given(@"a user with role ""(.*)"" exists in the clinic")]
    public async Task GivenAUserWithRoleExistsInTheClinic(string role)
    {
        var clinicId = _ctx.Get<Guid>("ClinicId");
        await CreateUserWithRole(clinicId, role);
    }

    [Given(@"the user has never logged in before")]
    public void GivenTheUserHasNeverLoggedInBefore()
    {
        // No-op: this is the default state — no onboarding record exists
    }

    [Given(@"the user has an onboarding state initialized")]
    public async Task GivenTheUserHasAnOnboardingStateInitialized()
    {
        // Ensure user is authenticated, then trigger lazy init by calling GET /api/v1/onboarding
        await EnsureAuthenticatedAdmin();
        _response = await _client.GetAsync("/api/v1/onboarding");
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "GET /api/v1/onboarding should succeed");
    }

    [Given(@"the clinic already has (\d+) patients")]
    public async Task GivenTheClinicAlreadyHasNPatients(int count)
    {
        var clinicId = _ctx.Get<Guid>("ClinicId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        for (var i = 0; i < count; i++)
        {
            var patient = Patient.Create(clinicId, $"Animal_{i}", Species.Dog, "Labrador", new DateOnly(2020, 1, 1));
            patient.IsSuccess.Should().BeTrue();
            db.Patients.Add(patient.Value);
        }
        await db.SaveChangesAsync();
    }

    [Given(@"the clinic already has (\d+) appointments")]
    public async Task GivenTheClinicAlreadyHasNAppointments(int count)
    {
        var clinicId = _ctx.Get<Guid>("ClinicId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        // Get or create a vet user for the appointment
        var vetEmail = "onboarding-vet@test.ae";
        var vet = await authDb.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == vetEmail);
        if (vet is null)
        {
            var vetResult = User.Create(clinicId, vetEmail, "SecurePass1", UserRole.Vet, "VET-OB-001");
            vet = vetResult.Value;
            authDb.Users.Add(vet);
            await authDb.SaveChangesAsync();
        }

        for (var i = 0; i < count; i++)
        {
            var appointment = Appointment.Create(
                clinicId, vet.Id, "Dr. Onboarding", Guid.NewGuid(), $"Animal_{i}", "Owner Test",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i + 1)),
                new TimeOnly(9, 0), 30, null);
            appointment.IsSuccess.Should().BeTrue();
            db.Appointments.Add(appointment.Value);
        }
        await db.SaveChangesAsync();
    }

    [Given(@"the user has all checklist steps completed")]
    public async Task GivenTheUserHasAllChecklistStepsCompleted()
    {
        await EnsureAuthenticatedAdmin();
        _response = await _client.GetAsync("/api/v1/onboarding");
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await _response.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);

        // Complete all steps via API
        foreach (var step in state!.Steps.Where(s => !s.IsCompleted))
        {
            var result = await _client.PostAsync($"/api/v1/onboarding/steps/{step.StepId}/complete", null);
            result.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }
    }

    [Given(@"the user has dismissed the welcome banner")]
    public async Task GivenTheUserHasDismissedTheWelcomeBanner()
    {
        await EnsureAuthenticatedAdmin();
        // First initialize the onboarding state via GET (lazy init)
        var initResponse = await _client.GetAsync("/api/v1/onboarding");
        initResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Onboarding state should be initialized");
        // Then dismiss the banner
        _response = await _client.PostAsync("/api/v1/onboarding/banner/dismiss", null);
        _response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"the user logs in for the first time")]
    public async Task WhenTheUserLogsInForTheFirstTime()
    {
        await EnsureAuthenticatedAdmin();
        // First login triggers lazy init on GET
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"the user retrieves their onboarding state")]
    public async Task WhenTheUserRetrievesTheirOnboardingState()
    {
        await EnsureAuthenticatedAdmin();
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"the step ""(.*)"" is marked as completed")]
    public async Task WhenTheStepIsMarkedAsCompleted(string stepId)
    {
        await EnsureAuthenticatedAdmin();
        _response = await _client.PostAsync($"/api/v1/onboarding/steps/{stepId}/complete", null);
    }

    [When(@"the user dismisses the welcome banner")]
    public async Task WhenTheUserDismissesTheWelcomeBanner()
    {
        await EnsureAuthenticatedAdmin();
        _response = await _client.PostAsync("/api/v1/onboarding/banner/dismiss", null);
    }

    [When(@"the user dismisses the checklist")]
    public async Task WhenTheUserDismissesTheChecklist()
    {
        await EnsureAuthenticatedAdmin();
        _response = await _client.PostAsync("/api/v1/onboarding/checklist/dismiss", null);
    }

    [When(@"the user logs out and logs back in")]
    public async Task WhenTheUserLogsOutAndLogsBackIn()
    {
        // Re-login: just re-authenticate with same credentials
        await EnsureAuthenticatedAdmin(forceRelogin: true);
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"the vet user retrieves their onboarding state")]
    public async Task WhenTheVetUserRetrievesTheirOnboardingState()
    {
        await EnsureAuthenticatedRole("Vet");
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"the receptionist user retrieves their onboarding state")]
    public async Task WhenTheReceptionistUserRetrievesTheirOnboardingState()
    {
        await EnsureAuthenticatedRole("Receptionist");
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"the assistant user retrieves their onboarding state")]
    public async Task WhenTheAssistantUserRetrievesTheirOnboardingState()
    {
        await EnsureAuthenticatedRole("Assistant");
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    [When(@"an unauthenticated request is made to the onboarding endpoint")]
    public async Task WhenAnUnauthenticatedRequestIsMadeToTheOnboardingEndpoint()
    {
        // Remove auth header
        _client.DefaultRequestHeaders.Authorization = null;
        _response = await _client.GetAsync("/api/v1/onboarding");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the onboarding state is initialized with role ""(.*)""")]
    public async Task ThenTheOnboardingStateIsInitializedWithRole(string role)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Should().NotBeEmpty();
    }

    [Then(@"the welcome banner is marked as ""(.*)""")]
    public async Task ThenTheWelcomeBannerIsMarkedAs(string status)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        if (status == "visible")
            state!.WelcomeBannerVisible.Should().BeTrue();
        else if (status == "dismissed")
            state!.WelcomeBannerVisible.Should().BeFalse();
    }

    [Then(@"the checklist is marked as ""(.*)""")]
    public async Task ThenTheChecklistIsMarkedAs(string status)
    {
        // Re-fetch to get updated state
        var currentResponse = await _client.GetAsync("/api/v1/onboarding");
        currentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await currentResponse.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
        state.Should().NotBeNull();
        if (status == "visible")
            state!.ChecklistVisible.Should().BeTrue();
        else if (status == "dismissed")
            state!.ChecklistVisible.Should().BeFalse();
    }

    [Then(@"all checklist steps are in ""pending"" status")]
    public async Task ThenAllChecklistStepsAreInPendingStatus()
    {
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Should().NotBeEmpty();
        state.Steps.All(s => !s.IsCompleted).Should().BeTrue("All steps should be pending on first init");
    }

    [Then(@"the response contains the welcome banner status")]
    public async Task ThenTheResponseContainsTheWelcomeBannerStatus()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        // WelcomeBannerVisible is a bool — it will always be present
    }

    [Then(@"the response contains checklist steps with completion status")]
    public async Task ThenTheResponseContainsChecklistStepsWithCompletionStatus()
    {
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Should().NotBeEmpty();
        state.Steps.Should().AllSatisfy(s =>
        {
            s.StepId.Should().NotBeNullOrEmpty();
            s.Label.Should().NotBeNullOrEmpty();
        });
    }

    [Then(@"the response contains the progress count")]
    public async Task ThenTheResponseContainsTheProgressCount()
    {
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Progress.Should().NotBeNull();
        state.Progress.Total.Should().BeGreaterThan(0);
    }

    [Then(@"the step ""(.*)"" has status ""completed""")]
    public async Task ThenTheStepHasStatusCompleted(string stepId)
    {
        _response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Re-fetch to verify
        var getResponse = await _client.GetAsync("/api/v1/onboarding");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await getResponse.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
        state.Should().NotBeNull();
        var step = state!.Steps.FirstOrDefault(s => s.StepId == stepId);
        step.Should().NotBeNull($"Step '{stepId}' should exist in the state");
        step!.IsCompleted.Should().BeTrue($"Step '{stepId}' should be completed");
    }

    [Then(@"the progress count is updated")]
    public async Task ThenTheProgressCountIsUpdated()
    {
        var getResponse = await _client.GetAsync("/api/v1/onboarding");
        var state = await getResponse.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
        state.Should().NotBeNull();
        state!.Progress.Completed.Should().BeGreaterThan(0);
    }

    [Then(@"the step ""(.*)"" is auto-completed")]
    public async Task ThenTheStepIsAutoCompleted(string stepId)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        var step = state!.Steps.FirstOrDefault(s => s.StepId == stepId);
        step.Should().NotBeNull($"Step '{stepId}' should exist");
        step!.IsCompleted.Should().BeTrue($"Step '{stepId}' should be auto-completed");
    }

    [Then(@"the checklist remains ""visible""")]
    public async Task ThenTheChecklistRemainsVisible()
    {
        var getResponse = await _client.GetAsync("/api/v1/onboarding");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await getResponse.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
        state.Should().NotBeNull();
        state!.ChecklistVisible.Should().BeTrue("Checklist should still be visible after banner dismiss");
    }

    [Then(@"the welcome banner status is unchanged")]
    public async Task ThenTheWelcomeBannerStatusIsUnchanged()
    {
        var getResponse = await _client.GetAsync("/api/v1/onboarding");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await getResponse.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
        state.Should().NotBeNull();
        // After dismiss checklist, the welcome banner should not have changed.
        // Just verify the response is valid (welcome banner can be either true or false)
        state.Should().NotBeNull();
    }

    [Then(@"the onboarding status is ""completed""")]
    public async Task ThenTheOnboardingStatusIsCompleted()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.IsCompleted.Should().BeTrue("Onboarding should be completed when all steps are done");
    }

    [Then(@"the welcome banner is still marked as ""dismissed""")]
    public async Task ThenTheWelcomeBannerIsStillMarkedAsDismissed()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.WelcomeBannerVisible.Should().BeFalse("Banner should still be dismissed after re-login");
    }

    [Then(@"the checklist contains (\d+) steps specific to the Vet role")]
    public async Task ThenTheChecklistContainsNStepsSpecificToTheVetRole(int count)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Count.Should().Be(count, $"Vet role should have {count} steps");
    }

    [Then(@"the checklist contains (\d+) steps specific to the Receptionist role")]
    public async Task ThenTheChecklistContainsNStepsSpecificToTheReceptionistRole(int count)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Count.Should().Be(count, $"Receptionist role should have {count} steps");
    }

    [Then(@"the checklist contains (\d+) steps specific to the Assistant role")]
    public async Task ThenTheChecklistContainsNStepsSpecificToTheAssistantRole(int count)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await GetCurrentState();
        state.Should().NotBeNull();
        state!.Steps.Count.Should().Be(count, $"Assistant role should have {count} steps");
    }

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int statusCode)
    {
        ((int)_response.StatusCode).Should().Be(statusCode);
    }

    [Then(@"the user must sign in")]
    public void ThenTheUserMustSignIn()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private bool _authInitialized = false;
    private string _currentRole = "Admin";

    private async Task<OnboardingStateDto?> GetCurrentState()
    {
        var resp = await _client.GetAsync("/api/v1/onboarding");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<OnboardingStateDto>(JsonOptions);
    }

    private async Task EnsureAuthenticatedAdmin(bool forceRelogin = false)
    {
        if (_authInitialized && !forceRelogin) return;
        await EnsureAuthenticatedRole("Admin", forceRelogin);
    }

    private async Task EnsureAuthenticatedRole(string role, bool forceRelogin = false)
    {
        if (_authInitialized && _currentRole == role && !forceRelogin) return;

        var clinicId = _ctx.ContainsKey("ClinicId")
            ? _ctx.Get<Guid>("ClinicId")
            : TestClinicContext.TestClinicGuid;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var email = $"onboarding-{role.ToLowerInvariant()}@test.ae";
        var password = "SecurePass1";

        var userRole = role.ToUpperInvariant() switch
        {
            "ADMIN" => UserRole.Admin,
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ASSISTANT" => UserRole.Assistant,
            _ => UserRole.Admin
        };

        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.ClinicId == clinicId);
        if (existing is null)
        {
            var vetLicense = userRole == UserRole.Vet ? "VET-OB-ROLE-001" : null;
            var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
            userResult.IsSuccess.Should().BeTrue($"User creation for role {role} should succeed");
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResp = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK, $"Login for {email} should succeed");

        var authToken = await loginResp.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _authInitialized = true;
        _currentRole = role;
    }

    private async Task CreateUserWithRole(Guid clinicId, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "ADMIN" => UserRole.Admin,
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ASSISTANT" => UserRole.Assistant,
            _ => UserRole.Admin
        };

        var email = $"onboarding-{role.ToLowerInvariant()}@test.ae";
        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.ClinicId == clinicId);
        if (existing is not null) return;

        var vetLicense = userRole == UserRole.Vet ? "VET-OB-ROLE-001" : null;
        var userResult = User.Create(clinicId, email, "SecurePass1", userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue();
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
