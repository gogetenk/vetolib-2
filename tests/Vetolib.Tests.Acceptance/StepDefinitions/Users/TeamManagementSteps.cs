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
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Users;

[Binding]
[Scope(Feature = "Team Management")]
internal class TeamManagementSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private string? _errorBody;
    private InviteUserResponse? _inviteResponse;
    private IReadOnlyList<UserListItemDto>? _userList;

    private Guid _clinicId;
    private Guid _adminUserId;
    private string _adminEmail = null!;
    private string _adminPassword = "Admin1234!";

    private readonly Dictionary<string, string> _userPasswords = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TeamManagementSteps(ScenarioContext ctx) => _ctx = ctx;

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN steps ─────────────────────────────────────────────

    [Given(@"I am a clinic admin ""(.*)"" in clinic ""(.*)""")]
    public async Task GivenAdminInClinic(string adminEmail, string clinicIdentifier)
    {
        _clinicId = TestClinicContext.TestClinicGuid;
        _adminEmail = adminEmail;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var result = User.Create(_clinicId, adminEmail, _adminPassword, UserRole.Admin, null);
        result.IsSuccess.Should().BeTrue();

        db.Users.Add(result.Value);
        await db.SaveChangesAsync();

        _adminUserId = result.Value.Id;
        _userPasswords[adminEmail] = _adminPassword;

        await LoginAs(adminEmail, _adminPassword);
    }

    [Given(@"the team has a vet ""(.*)"" and a receptionist ""(.*)""")]
    public async Task GivenTeamHasVetAndReceptionist(string vetEmail, string receptionistEmail)
    {
        await CreateUserInDb(vetEmail, "Vet1234!", UserRole.Vet, "VET-LIC-001");
        await CreateUserInDb(receptionistEmail, "Recep1234!", UserRole.Receptionist, null);
    }

    [Given(@"there is a user ""(.*)"" with role ""(.*)""")]
    public async Task GivenUserWithRole(string email, string roleName)
    {
        var role = Enum.Parse<UserRole>(roleName);
        var vetLicense = role == UserRole.Vet ? "VET-LIC-TEST" : null;
        await CreateUserInDb(email, "User1234!", role, vetLicense);
    }

    [Given(@"there is an active user ""(.*)"" with role ""(.*)""")]
    public async Task GivenActiveUserWithRole(string email, string roleName)
    {
        var role = Enum.Parse<UserRole>(roleName);
        var vetLicense = role == UserRole.Vet ? "VET-LIC-TEST" : null;
        await CreateUserInDb(email, "User1234!", role, vetLicense);
    }

    [Given(@"I am a vet ""(.*)"" in clinic ""(.*)""")]
    public async Task GivenVetInClinic(string vetEmail, string clinicIdentifier)
    {
        _clinicId = TestClinicContext.TestClinicGuid;
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;

        var vetPassword = "Vet1234!";
        await CreateUserInDb(vetEmail, vetPassword, UserRole.Vet, "VET-LIC-001");

        // Switch auth to vet
        await LoginAs(vetEmail, vetPassword);
    }

    [Given(@"""(.*)"" has been deactivated by admin")]
    public async Task GivenUserDeactivated(string email)
    {
        _response = await _client.DeleteAsync($"/api/v1/users/{await GetUserId(email)}");
        _response.IsSuccessStatusCode.Should().BeTrue($"Deactivation should succeed for {email}");
    }

    // ─── WHEN steps ──────────────────────────────────────────────

    [When(@"I list all team members")]
    public async Task WhenListAllTeamMembers()
    {
        _response = await _client.GetAsync("/api/v1/users");
        if (_response.IsSuccessStatusCode)
        {
            var paged = await _response.Content.ReadFromJsonAsync<UserPagedResultDto>(JsonOptions);
            _userList = paged?.Items?.ToList();
        }
        else
            _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I invite a new user with email ""(.*)"", name ""(.*)"", role ""(.*)""")]
    public async Task WhenInviteUser(string email, string fullName, string roleName)
    {
        var role = Enum.Parse<UserRole>(roleName);
        var request = new InviteUserRequest(email, fullName, role);
        _response = await _client.PostAsJsonAsync("/api/v1/users/invite", request);

        if (_response.IsSuccessStatusCode)
            _inviteResponse = await _response.Content.ReadFromJsonAsync<InviteUserResponse>(JsonOptions);
        else
            _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I change ""(.*)"" role to ""(.*)""")]
    public async Task WhenChangeRole(string email, string newRoleName)
    {
        var userId = await GetUserId(email);
        var newRole = Enum.Parse<UserRole>(newRoleName);
        var request = new ChangeRoleRequest(newRole);
        _response = await _client.PatchAsJsonAsync($"/api/v1/users/{userId}/role", request);

        if (!_response.IsSuccessStatusCode)
            _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I deactivate ""(.*)""")]
    public async Task WhenDeactivateUser(string email)
    {
        var userId = await GetUserId(email);
        _response = await _client.DeleteAsync($"/api/v1/users/{userId}");

        if (!_response.IsSuccessStatusCode)
            _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I try to deactivate myself")]
    public async Task WhenDeactivateSelf()
    {
        _response = await _client.DeleteAsync($"/api/v1/users/{_adminUserId}");
        _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"a vet lists all team members")]
    public async Task WhenVetListsAllTeamMembers()
    {
        _response = await _client.GetAsync("/api/v1/users");
        _errorBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"""(.*)"" tries to log in")]
    public async Task WhenUserTriesToLogin(string email)
    {
        // Use a fresh client (unauthenticated)
        var freshClient = _factory.CreateClient();
        _response = await freshClient.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "User1234!"));
        _errorBody = await _response.Content.ReadAsStringAsync();
    }

    // ─── THEN steps ──────────────────────────────────────────────

    [Then(@"I receive a list with 3 members")]
    public void ThenListHas3Members()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _userList.Should().NotBeNull();
        _userList!.Count.Should().Be(3);
    }

    [Then(@"the invitation succeeds")]
    public void ThenInvitationSucceeds()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Invite failed: {_errorBody}");
        _inviteResponse.Should().NotBeNull();
    }

    [Then(@"a temporary password is generated")]
    public void ThenTemporaryPasswordIsGenerated()
    {
        _inviteResponse.Should().NotBeNull();
        _inviteResponse!.TemporaryPassword.Should().NotBeNullOrEmpty();
        _inviteResponse.TemporaryPassword.Length.Should().BeGreaterThanOrEqualTo(8);
    }

    [Then(@"the new user appears in the team list")]
    public async Task ThenNewUserInList()
    {
        var listResponse = await _client.GetAsync("/api/v1/users");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await listResponse.Content.ReadFromJsonAsync<UserPagedResultDto>(JsonOptions);
        paged.Should().NotBeNull();
        paged!.Items.Should().Contain(u => u.Email == _inviteResponse!.Email);
    }

    [Then(@"the role change succeeds")]
    public void ThenRoleChangeSucceeds()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Role change failed: {_errorBody}");
    }

    [Then(@"""(.*)"" has role ""(.*)""")]
    public async Task ThenUserHasRole(string email, string roleName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == email.ToLowerInvariant());

        var expectedRole = Enum.Parse<UserRole>(roleName);
        user.Role.Should().Be(expectedRole);
    }

    [Then(@"the deactivation succeeds")]
    public void ThenDeactivationSucceeds()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, $"Deactivation failed: {_errorBody}");
    }

    [Then(@"""(.*)"" is inactive")]
    public async Task ThenUserIsInactive(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == email.ToLowerInvariant());
        user.IsActive.Should().BeFalse();
    }

    [Then(@"the request is rejected with message ""(.*)""")]
    public void ThenRequestRejectedWithMessage(string expectedMessage)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _errorBody.Should().Contain(expectedMessage);
    }

    [Then(@"the user is denied access")]
    public void ThenUserIsDeniedAccess()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Then(@"login fails with message ""(.*)""")]
    public void ThenLoginFailsWithMessage(string expectedMessage)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();
        _errorBody.Should().Contain(expectedMessage);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task LoginAs(string email, string password)
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var token = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

    private async Task CreateUserInDb(string email, string password, UserRole role, string? vetLicense)
    {
        _userPasswords[email] = password;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var result = User.Create(_clinicId, email, password, role, vetLicense);
        result.IsSuccess.Should().BeTrue($"User creation failed for {email}");

        db.Users.Add(result.Value);
        await db.SaveChangesAsync();
    }

    private async Task<Guid> GetUserId(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == email.ToLowerInvariant());
        return user.Id;
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
