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
[Scope(Feature = "Authentification et gestion des tokens JWT")]
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

    [Given(@"une clinique ""(.*)"" avec l'identifiant ""(.*)""")]
    public void GivenUneClinique(string clinicName, string clinicIdentifier)
    {
        // Generate a stable GUID from the identifier
        var clinicId = GenerateGuidFromString(clinicIdentifier);
        _clinicIds[clinicIdentifier] = clinicId;

        // Update the test clinic context
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        // Default to the first clinic created
        if (_clinicIds.Count == 1)
        {
            testClinicContext.ClinicId = clinicId;
        }
    }

    [Given(@"un utilisateur existant avec les informations suivantes:")]
    public async Task GivenUnUtilisateurExistant(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            await CreateUserInDb(row);
        }
    }

    [Given(@"un utilisateur admin existant:")]
    public async Task GivenUnUtilisateurAdminExistant(DataTable table)
    {
        foreach (var row in table.Rows)
        {
            await CreateUserInDb(row);
        }
    }

    [Given(@"je suis connecte en tant que ""(.*)""")]
    public async Task GivenJeSuisConnecteEnTantQue(string email)
    {
        var password = _userPasswords[email];
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();
        _authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authToken!.AccessToken);
    }

    [Given(@"je possede un refresh token valide")]
    public void GivenJePossedeUnRefreshTokenValide()
    {
        _authToken.Should().NotBeNull("I should be logged in first");
        _authToken!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Given(@"le compte ""(.*)"" est verrouille suite a 5 tentatives echouees")]
    public async Task GivenLeCompteEstVerrouille(string email)
    {
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(email, "WrongPassword1!"));
        }
    }

    [Given(@"le compte ""(.*)"" a ete verrouille il y a 16 minutes")]
    public async Task GivenLeCompteAEteVerrouilleIlYa16Minutes(string email)
    {
        // First, lock the account
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/auth/login",
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

    [Given(@"mon refresh token a ete revoque par un precedent refresh")]
    public async Task GivenMonRefreshTokenAEteRevoqueParUnPrecedentRefresh()
    {
        _previousAuthToken = _authToken;

        // Perform a refresh to rotate the token (which revokes the old one)
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(_authToken!.RefreshToken));
        refreshResponse.EnsureSuccessStatusCode();
        _authToken = await refreshResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
    }

    [Given(@"mon refresh token a expire depuis plus de 7 jours")]
    public async Task GivenMonRefreshTokenAExpire()
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

    [When(@"je me connecte avec l'email ""(.*)"" et le mot de passe ""(.*)""")]
    public async Task WhenJeMeConnecte(string email, string password)
    {
        _response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));

        if (_response.IsSuccessStatusCode)
        {
            _authToken = await _response.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"je me connecte 5 fois avec l'email ""(.*)"" et un mot de passe incorrect")]
    public async Task WhenJeMeConnecte5Fois(string email)
    {
        for (int i = 0; i < 5; i++)
        {
            _response = await _client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(email, "WrongPassword1!"));
        }
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"^j'appelle POST /api/auth/refresh avec mon refresh token$")]
    public async Task WhenJAppelleRefreshAvecMonRefreshToken()
    {
        _previousAuthToken = _authToken;
        _response = await _client.PostAsJsonAsync("/api/auth/refresh",
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

    [When(@"^j'appelle POST /api/auth/refresh avec le refresh token revoque$")]
    public async Task WhenJAppelleRefreshAvecLeRefreshTokenRevoque()
    {
        _response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"^j'appelle POST /api/auth/refresh avec le refresh token expire$")]
    public async Task WhenJAppelleRefreshAvecLeRefreshTokenExpire()
    {
        _response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(_authToken!.RefreshToken));
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"^j'appelle POST /api/auth/logout$")]
    public async Task WhenJAppelleLogout()
    {
        _response = await _client.PostAsync("/api/auth/logout", null);

        if (_response.IsSuccessStatusCode)
        {
            _previousAuthToken = _authToken;
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"^j'appelle GET /api/auth/me$")]
    public async Task WhenJAppelleGetMe()
    {
        _response = await _client.GetAsync("/api/auth/me");

        if (_response.IsSuccessStatusCode)
        {
            // Response will be checked in Then steps
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"^j'appelle GET /api/auth/me sans token d'authentification$")]
    public async Task WhenJAppelleGetMeSansToken()
    {
        var unauthClient = _factory.CreateClient();
        _response = await unauthClient.GetAsync("/api/auth/me");
    }

    [When(@"je cree un utilisateur avec les informations suivantes:")]
    public async Task WhenJeCreerUnUtilisateur(DataTable table)
    {
        var row = table.Rows[0];
        var request = new CreateUserRequest(
            row["Email"],
            row["Password"],
            Enum.Parse<UserRole>(row["Role"]),
            row.ContainsKey("VetLicenseNumber") && !string.IsNullOrEmpty(row["VetLicenseNumber"])
                ? row["VetLicenseNumber"]
                : null);

        _response = await _client.PostAsJsonAsync("/api/users", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdUser = await _response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"je tente de creer un utilisateur avec les informations suivantes:")]
    public async Task WhenJeTenteDeCreerUnUtilisateur(DataTable table)
    {
        var row = table.Rows[0];
        var vetLicense = row.ContainsKey("VetLicenseNumber") ? row["VetLicenseNumber"] : null;
        if (string.IsNullOrEmpty(vetLicense)) vetLicense = null;

        var request = new CreateUserRequest(
            row["Email"],
            row["Password"],
            Enum.Parse<UserRole>(row["Role"]),
            vetLicense);

        _response = await _client.PostAsJsonAsync("/api/users", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un utilisateur avec l'email ""(.*)"" et le mot de passe ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnUtilisateurAvecEmailEtPassword(string email, string password)
    {
        var request = new CreateUserRequest(email, password, UserRole.Receptionist, null);
        _response = await _client.PostAsJsonAsync("/api/users", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"je recois un access token JWT valide")]
    public void ThenJeRecoisUnAccessTokenJWTValide()
    {
        _authToken.Should().NotBeNull();
        _authToken!.AccessToken.Should().NotBeNullOrEmpty();

        // Validate it's a proper JWT
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(_authToken.AccessToken).Should().BeTrue();
    }

    [Then(@"je recois un refresh token")]
    public void ThenJeRecoisUnRefreshToken()
    {
        _authToken.Should().NotBeNull();
        _authToken!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Then(@"la reponse contient les informations utilisateur:")]
    public void ThenLaReponseContientLesInfosUtilisateur(DataTable table)
    {
        var row = table.Rows[0];
        _authToken.Should().NotBeNull();
        _authToken!.User.Email.Should().Be(row["Email"]);
        _authToken.User.Role.ToString().Should().Be(row["Role"]);
        _authToken.User.ClinicId.Should().Be(_clinicIds[row["ClinicId"]]);
        if (row.ContainsKey("VetLicenseNumber"))
            _authToken.User.VetLicenseNumber.Should().Be(row["VetLicenseNumber"]);
    }

    [Then(@"le JWT contient le claim ""(.*)"" avec la valeur ""(.*)""")]
    public void ThenLeJWTContientLeClaim(string claimName, string expectedValue)
    {
        _authToken.Should().NotBeNull();
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_authToken!.AccessToken);
        var claim = token.Claims.FirstOrDefault(c => c.Type == claimName);
        claim.Should().NotBeNull($"JWT should contain claim '{claimName}'");

        var expectedGuid = _clinicIds.ContainsKey(expectedValue)
            ? _clinicIds[expectedValue].ToString()
            : expectedValue;
        claim!.Value.Should().Be(expectedGuid);
    }

    [Then(@"le access token a une duree de validite de 15 minutes")]
    public void ThenLeAccessTokenExpireApres15Minutes()
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

    [Then(@"je recois un nouveau access token JWT valide")]
    public void ThenJeRecoisUnNouveauAccessTokenJWTValide()
    {
        _authToken.Should().NotBeNull();
        _authToken!.AccessToken.Should().NotBeNullOrEmpty();
        _authToken.AccessToken.Should().NotBe(_previousAuthToken!.AccessToken);
    }

    [Then(@"je recois un nouveau refresh token different de l'ancien")]
    public void ThenJeRecoisUnNouveauRefreshTokenDifferent()
    {
        _authToken.Should().NotBeNull();
        _authToken!.RefreshToken.Should().NotBe(_previousAuthToken!.RefreshToken);
    }

    [Then(@"l'ancien refresh token est invalide")]
    public async Task ThenLancienRefreshTokenEstInvalide()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        response.IsSuccessStatusCode.Should().BeFalse("Old refresh token should be invalid");
    }

    [Then(@"le nouveau refresh token a une duree de validite de 7 jours")]
    public async Task ThenLeNouveauRefreshTokenExpireApres7Jours()
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

    [Then(@"la deconnexion est confirmee")]
    public void ThenLaDeconnexionEstConfirmee()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"le refresh token est invalide")]
    public async Task ThenLeRefreshTokenEstInvalide()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == _previousAuthToken!.User.Id)
            .ToListAsync();

        tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Then(@"une tentative de refresh avec cet ancien token echoue")]
    public async Task ThenUneTentativeDeRefreshAvecCetAncienTokenEchoue()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(_previousAuthToken!.RefreshToken));
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Then(@"je recois les informations de mon profil:")]
    public async Task ThenJeRecoisLesInformationsDuProfil(DataTable table)
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

    [Then(@"le systeme refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
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

    [Then(@"le message est ""(.*)""")]
    public void ThenLeMessageEst(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"le message indique que le compte est verrouille pour 15 minutes")]
    public void ThenLeMessageIndiqueVerrouillage15Minutes()
    {
        _errorResponseBody.Should().Contain("verrouille");
        _errorResponseBody.Should().Contain("15 minutes");
    }

    [Then(@"le message indique que le compte est verrouille")]
    public void ThenLeMessageIndiqueVerrouillage()
    {
        _errorResponseBody.Should().Contain("verrouille");
    }

    [Then(@"le compteur de tentatives echouees est reinitialise")]
    public async Task ThenLeCompteurEstReinitialise()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Email == "vet@happypaws.ae");
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
    }

    [Then(@"le systeme retourne le code HTTP (.*)")]
    public void ThenLeSystemeRetourneLeCodeHTTP(int statusCode)
    {
        ((int)_response.StatusCode).Should().Be(statusCode);
    }

    [Then(@"les requetes de cet utilisateur ne retournent que les donnees de ""(.*)""")]
    public void ThenLesRequetesNeRetournentQueLesData(string clinicIdentifier)
    {
        _authToken.Should().NotBeNull();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(_authToken!.AccessToken);
        var clinicClaim = token.Claims.First(c => c.Type == "clinic_id");

        var expectedClinicId = _clinicIds[clinicIdentifier].ToString();
        clinicClaim.Value.Should().Be(expectedClinicId);
    }

    [Then(@"l'utilisateur est cree avec succes")]
    public void ThenLUtilisateurEstCreeAvecSucces()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _createdUser.Should().NotBeNull();
    }

    [Then(@"l'utilisateur cree appartient a la clinique ""(.*)""")]
    public void ThenLUtilisateurAppartientALaClinique(string clinicIdentifier)
    {
        _createdUser.Should().NotBeNull();
        _createdUser!.ClinicId.Should().Be(_clinicIds[clinicIdentifier]);
    }

    [Then(@"le message contient ""(.*)""")]
    public void ThenLeMessageContient(string expectedPart)
    {
        _errorResponseBody.Should().Contain(expectedPart);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task CreateUserInDb(DataTableRow row)
    {
        var clinicIdentifier = row["ClinicId"];
        if (!_clinicIds.ContainsKey(clinicIdentifier))
        {
            _clinicIds[clinicIdentifier] = GenerateGuidFromString(clinicIdentifier);
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
