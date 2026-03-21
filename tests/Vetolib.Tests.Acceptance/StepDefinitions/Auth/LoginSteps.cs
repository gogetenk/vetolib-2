using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "Authentication and JWT token management")]
internal class LoginSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private AuthTokenDto? _authToken;
    private AuthTokenDto? _previousAuthToken;
    private string? _errorResponseBody;
    private readonly Dictionary<string, Guid> _clinicIds = new();
    private readonly Dictionary<string, string> _userPasswords = new();
    private UserDto? _createdUser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public LoginSteps(ScenarioContext ctx)
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

    [Given(@"a clinic ""(.*)"" with identifier ""(.*)""")]
    public void GivenAClinicWithIdentifier(string clinicName, string clinicIdentifier)
    {
        // First clinic gets the fixed TestClinicGuid; subsequent ones get unique GUIDs
        // so that tenant-isolation queries work correctly.
        var clinicId = _clinicIds.Count == 0
            ? TestClinicContext.TestClinicGuid
            : GenerateGuidFromString(clinicIdentifier);
        _clinicIds[clinicIdentifier] = clinicId;

        // Update the test clinic context
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        // Default to the first clinic created
        if (_clinicIds.Count == 1)
        {
            testClinicContext.ClinicId = clinicId;
        }
    }

    [Given(@"an existing user with the following information:")]
    public async Task GivenAnExistingUserWithTheFollowingInformation(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            await CreateUserInDb(row);
        }
    }

    [Given(@"an existing admin user:")]
    public async Task GivenAnExistingAdminUser(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            await CreateUserInDb(row);
        }
    }

    [Given(@"I am logged in as ""(.*)""")]
    public async Task GivenIAmLoggedInAs(string email)
    {
        var password = _userPasswords[email];
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();
        _authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authToken!.AccessToken);
    }

    [Given(@"I have a valid refresh token")]
    public void GivenIHaveAValidRefreshToken()
    {
        _authToken.Should().NotBeNull("I should be logged in first");
        _authToken!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Given(@"the account ""(.*)"" is locked after 5 failed attempts")]
    public async Task GivenTheAccountIsLockedAfter5FailedAttempts(string email)
    {
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(email, "WrongPassword1!"));
        }
    }

    [Given(@"the account ""(.*)"" was locked 16 minutes ago")]
    public async Task GivenTheAccountWasLocked16MinutesAgo(string email)
    {
        // First, lock the account
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(email, "WrongPassword1!"));
        }

        // Then set LockedUntil to 16 minutes ago directly in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == email.ToLowerInvariant());

        // Use reflection to set LockedUntil since properties are private set
        var lockedUntilProp = typeof(User).GetProperty("LockedUntil")!;
        lockedUntilProp.SetValue(user, DateTime.UtcNow.AddMinutes(-1));
        await db.SaveChangesAsync();
    }

    [Given(@"my refresh token has been revoked by a previous refresh")]
    public async Task GivenMyRefreshTokenHasBeenRevokedByAPreviousRefresh()
    {
        _previousAuthToken = _authToken;

        // Perform a refresh to rotate the token (which revokes the old one)
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_authToken!.RefreshToken));
        refreshResponse.EnsureSuccessStatusCode();
        _authToken = await refreshResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
    }

    [Given(@"my refresh token has expired for more than 7 days")]
    public async Task GivenMyRefreshTokenHasExpiredForMoreThan7Days()
    {
        // Set the refresh token expiry to the past directly in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var token = await db.RefreshTokens
            .FirstAsync(rt => rt.Token == _authToken!.RefreshToken);

        var expiresAtProp = typeof(RefreshToken).GetProperty("ExpiresAt")!;
        expiresAtProp.SetValue(token, DateTime.UtcNow.AddDays(-1));
        await db.SaveChangesAsync();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I log in with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenILogInWithEmailAndPassword(string email, string password)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
        {
            _authToken = await _response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
            _ctx.Set(_errorResponseBody, "ErrorResponseBody");
        }
    }

    [When(@"I log in 5 times with email ""(.*)"" and an incorrect password")]
    public async Task WhenILogIn5TimesWithEmailAndAnIncorrectPassword(string email)
    {
        for (int i = 0; i < 5; i++)
        {
            _response = await _client.PostAsJsonAsync("/api/v1/auth/login",
                new LoginRequest(email, "WrongPassword1!"));
        }
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"^the user refreshes their session$")]
    public async Task WhenTheUserRefreshesTheirSession()
    {
        _previousAuthToken = _authToken;
        _response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_authToken!.RefreshToken));

        if (_response.IsSuccessStatusCode)
        {
            _authToken = await _response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"^the user refreshes their session with the revoked refresh token$")]
    public async Task WhenTheUserRefreshesTheirSessionWithTheRevokedRefreshToken()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"^the user refreshes their session with the expired refresh token$")]
    public async Task WhenTheUserRefreshesTheirSessionWithTheExpiredRefreshToken()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_authToken!.RefreshToken));
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"^the user logs out$")]
    public async Task WhenTheUserLogsOut()
    {
        _response = await _client.PostAsync("/api/v1/auth/logout", null);

        if (_response.IsSuccessStatusCode)
        {
            _previousAuthToken = _authToken;
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"^the user checks their profile$")]
    public async Task WhenTheUserChecksTheirProfile()
    {
        _response = await _client.GetAsync("/api/v1/auth/me");

        if (_response.IsSuccessStatusCode)
        {
            // Response will be checked in Then steps
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"^an unauthenticated user checks their profile$")]
    public async Task WhenAnUnauthenticatedUserChecksTheirProfile()
    {
        var unauthClient = _factory.CreateClient();
        _response = await unauthClient.GetAsync("/api/v1/auth/me");
    }

    [When(@"I create a user with the following information:")]
    public async Task WhenICreateAUserWithTheFollowingInformation(DataTable table)
    {
        var row = table.Rows[0];
        var request = new CreateUserRequest(
            row["Email"],
            row["Password"],
            Enum.Parse<UserRole>(row["Role"]),
            row.ContainsKey("VetLicenseNumber") && !string.IsNullOrEmpty(row["VetLicenseNumber"])
                ? row["VetLicenseNumber"]
                : null);

        _response = await _client.PostAsJsonAsync("/api/v1/users", request);
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
        {
            _createdUser = await _response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
            _ctx.Set(_errorResponseBody, "ErrorResponseBody");
        }
    }

    [When(@"I attempt to create a user with the following information:")]
    public async Task WhenIAttemptToCreateAUserWithTheFollowingInformation(DataTable table)
    {
        var row = table.Rows[0];
        var vetLicense = row.ContainsKey("VetLicenseNumber") ? row["VetLicenseNumber"] : null;
        if (string.IsNullOrEmpty(vetLicense)) vetLicense = null;

        var request = new CreateUserRequest(
            row["Email"],
            row["Password"],
            Enum.Parse<UserRole>(row["Role"]),
            vetLicense);

        _response = await _client.PostAsJsonAsync("/api/v1/users", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"I attempt to create a user with email ""(.*)"" and password ""(.*)""")]
    public async Task WhenIAttemptToCreateAUserWithEmailAndPassword(string email, string password)
    {
        var request = new CreateUserRequest(email, password, UserRole.Receptionist, null);
        _response = await _client.PostAsJsonAsync("/api/v1/users", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I am successfully authenticated")]
    public void ThenIAmSuccessfullyAuthenticated()
    {
        _authToken.Should().NotBeNull();
        _authToken!.AccessToken.Should().NotBeNullOrEmpty();

        // Validate it's a proper JWT
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(_authToken.AccessToken).Should().BeTrue();
    }

    [Then(@"I receive a refresh token")]
    public void ThenIReceiveARefreshToken()
    {
        _authToken.Should().NotBeNull();
        _authToken!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"the response contains the user information:")]
    public void ThenTheResponseContainsTheUserInformation(DataTable table)
    {
        var row = table.Rows[0];
        _authToken.Should().NotBeNull();
        _authToken!.User.Email.Should().Be(row["Email"]);
        _authToken.User.Role.ToString().Should().Be(row["Role"]);
        _authToken.User.ClinicId.Should().Be(_clinicIds[row["ClinicId"]]);
        if (row.ContainsKey("VetLicenseNumber"))
            _authToken.User.VetLicenseNumber.Should().Be(row["VetLicenseNumber"]);
    }

    [Then(@"the session is linked to the clinic ""(.*)""")]
    public void ThenTheSessionIsLinkedToTheClinic(string expectedValue)
    {
        _authToken.Should().NotBeNull();
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_authToken!.AccessToken);
        var claim = token.Claims.FirstOrDefault(c => c.Type == "clinic_id");
        claim.Should().NotBeNull("JWT should contain claim 'clinic_id'");

        var expectedGuid = _clinicIds.ContainsKey(expectedValue)
            ? _clinicIds[expectedValue].ToString()
            : expectedValue;
        claim!.Value.Should().Be(expectedGuid);
    }

    [Then(@"the access token has a validity duration of 15 minutes")]
    public void ThenTheAccessTokenHasAValidityDurationOf15Minutes()
    {
        _authToken.Should().NotBeNull();
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_authToken!.AccessToken);

        var expiry = token.ValidTo;
        var issuedAt = token.ValidFrom;
        var duration = expiry - issuedAt;

        duration.TotalMinutes.Should().BeApproximately(15, 1,
            "Access token should expire after approximately 15 minutes");
    }

    [Then(@"I receive a new valid access token")]
    public void ThenIReceiveANewValidAccessToken()
    {
        _authToken.Should().NotBeNull();
        _authToken!.AccessToken.Should().NotBeNullOrEmpty();
        _authToken.AccessToken.Should().NotBe(_previousAuthToken!.AccessToken);
    }

    [Then(@"I receive a new refresh token different from the old one")]
    public void ThenIReceiveANewRefreshTokenDifferentFromTheOldOne()
    {
        _authToken.Should().NotBeNull();
        _authToken!.RefreshToken.Should().NotBe(_previousAuthToken!.RefreshToken);
    }

    [Then(@"the old refresh token is invalid")]
    public async Task ThenTheOldRefreshTokenIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        response.IsSuccessStatusCode.Should().BeFalse("Old refresh token should be invalid");
    }

    [Then(@"the new refresh token has a validity duration of 7 days")]
    public async Task ThenTheNewRefreshTokenHasAValidityDurationOf7Days()
    {
        _authToken.Should().NotBeNull();

        // Check in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var refreshToken = await db.RefreshTokens
            .OrderByDescending(rt => rt.CreatedAt)
            .FirstAsync(rt => rt.Token == _authToken!.RefreshToken);

        var duration = refreshToken.ExpiresAt - refreshToken.CreatedAt;
        duration.TotalDays.Should().BeApproximately(7, 0.1,
            "Refresh token should expire after approximately 7 days");
    }

    [Then(@"the logout is confirmed")]
    public void ThenTheLogoutIsConfirmed()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the refresh token is invalid")]
    public async Task ThenTheRefreshTokenIsInvalid()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == _previousAuthToken!.User.Id)
            .ToListAsync();

        tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Then(@"an attempt to refresh with the old token fails")]
    public async Task ThenAnAttemptToRefreshWithTheOldTokenFails()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"I receive my profile information:")]
    public async Task ThenIReceiveMyProfileInformation(DataTable table)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await _response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();

        var row = table.Rows[0];
        user!.Email.Should().Be(row["Email"]);
        user.Role.ToString().Should().Be(row["Role"]);
        user.ClinicId.Should().Be(_clinicIds[row["ClinicId"]]);
        if (row.ContainsKey("VetLicenseNumber"))
            user.VetLicenseNumber.Should().Be(row["VetLicenseNumber"]);
    }

    [Then(@"the system rejects with code ""(.*)""")]
    public void ThenTheSystemRejectsWithCode(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();

        switch (errorCode)
        {
            case "INVALID_CREDENTIALS":
                _errorResponseBody.Should().Contain("INVALID_CREDENTIALS");
                break;
            case "ACCOUNT_LOCKED":
                _errorResponseBody.Should().Contain("ACCOUNT_LOCKED");
                break;
            case "INVALID_REFRESH_TOKEN":
                _errorResponseBody.Should().Contain("INVALID_REFRESH_TOKEN");
                break;
            case "FORBIDDEN":
                _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
                break;
            case "VET_LICENSE_REQUIRED":
                _errorResponseBody.Should().Contain("VET_LICENSE_REQUIRED");
                break;
            case "VALIDATION_ERROR":
                _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                break;
        }
    }

    [Then(@"the error message is ""(.*)""")]
    public void ThenTheErrorMessageIs(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"the message indicates the account is locked for 15 minutes")]
    public void ThenTheMessageIndicatesTheAccountIsLockedFor15Minutes()
    {
        _errorResponseBody.Should().Contain("verrouille");
        _errorResponseBody.Should().Contain("15 minutes");
    }

    [Then(@"the message indicates the account is locked")]
    public void ThenTheMessageIndicatesTheAccountIsLocked()
    {
        _errorResponseBody.Should().Contain("verrouille");
    }

    [Then(@"the failed attempts counter is reset")]
    public async Task ThenTheFailedAttemptsCounterIsReset()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == "vet@happypaws.ae");
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
    }

    [Then(@"the user is denied access")]
    public void ThenTheUserIsDeniedAccess()
    {
        _response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Then(@"the requests from this user only return data from ""(.*)""")]
    public void ThenTheRequestsFromThisUserOnlyReturnDataFrom(string clinicIdentifier)
    {
        _authToken.Should().NotBeNull();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_authToken!.AccessToken);
        var clinicClaim = token.Claims.First(c => c.Type == "clinic_id");

        var expectedClinicId = _clinicIds[clinicIdentifier].ToString();
        clinicClaim.Value.Should().Be(expectedClinicId);
    }

    [Then(@"the user is created successfully")]
    public void ThenTheUserIsCreatedSuccessfully()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdUser.Should().NotBeNull();
    }

    [Then(@"the created user belongs to clinic ""(.*)""")]
    public void ThenTheCreatedUserBelongsToClinic(string clinicIdentifier)
    {
        _createdUser.Should().NotBeNull();
        _createdUser!.ClinicId.Should().Be(_clinicIds[clinicIdentifier]);
    }

    [Then(@"the message contains ""(.*)""")]
    public void ThenTheMessageContains(string reason)
    {
        _errorResponseBody.Should().Contain(reason);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task CreateUserInDb(DataTableRow row)
    {
        var clinicIdentifier = row["ClinicId"];
        if (!_clinicIds.ContainsKey(clinicIdentifier))
        {
            _clinicIds[clinicIdentifier] = _clinicIds.Count == 0
                ? TestClinicContext.TestClinicGuid
                : GenerateGuidFromString(clinicIdentifier);
        }
        var clinicId = _clinicIds[clinicIdentifier];

        var email = row["Email"];
        var password = row["Password"];
        var role = Enum.Parse<UserRole>(row["Role"]);
        var vetLicense = row.ContainsKey("VetLicenseNumber") ? row["VetLicenseNumber"] : null;

        _userPasswords[email] = password;

        // Set the clinic context for this user
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(clinicId, email, password, role, vetLicense);
        userResult.IsSuccess.Should().BeTrue($"User creation should succeed for {email}");

        db.Users.Add(userResult.Value);
        await db.SaveChangesAsync();
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private bool ContainsKey(DataTableRow row, string key)
    {
        try
        {
            var _ = row[key];
            return true;
        }
        catch
        {
            return false;
        }
    }
}
