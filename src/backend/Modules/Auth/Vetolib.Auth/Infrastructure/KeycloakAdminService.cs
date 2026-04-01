using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Infrastructure;

internal sealed class KeycloakAdminOptions
{
    public const string SectionName = "Keycloak";

    public string Realm { get; set; } = "vetolib";
    public string ClientId { get; set; } = "vetolib-api";
    public string ClientSecret { get; set; } = string.Empty;
}

internal sealed class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakAdminService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private string _cachedAccessToken = string.Empty;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public KeycloakAdminService(
        HttpClient httpClient,
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<Guid>> CreateUserAsync(
        string email, string password, string firstName, string lastName, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return Result<Guid>.Error(string.Join("; ", ensureAuth.Errors));

        var payload = new
        {
            username = email,
            email,
            firstName,
            lastName,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users", payload, JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return Result<Guid>.Conflict($"User with email '{email}' already exists in Keycloak.");

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult<Guid>(response, "CreateUser", ct);

        // Keycloak returns the user ID in the Location header
        var locationHeader = response.Headers.Location?.ToString();
        if (locationHeader is null)
            return Result<Guid>.Error("Keycloak did not return a Location header after user creation.");

        var userIdString = locationHeader.Split('/').LastOrDefault();
        if (!Guid.TryParse(userIdString, out var userId))
            return Result<Guid>.Error($"Could not parse user ID from Location header: {locationHeader}");

        _logger.LogInformation("Created Keycloak user {UserId} for {Email}", userId, email);
        return Result<Guid>.Success(userId);
    }

    public async Task<Result> AddUserToOrganizationAsync(
        Guid userId, Guid organizationId, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return ensureAuth;

        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/organizations/{organizationId}/members",
            new { userId = userId.ToString() },
            JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult(response, "AddUserToOrganization", ct);

        if (roles.Count > 0)
        {
            var rolesResult = await AssignOrganizationRolesAsync(userId, organizationId, roles, ct);
            if (!rolesResult.IsSuccess) return rolesResult;
        }

        _logger.LogInformation("Added user {UserId} to organization {OrgId} with roles [{Roles}]",
            userId, organizationId, string.Join(", ", roles));
        return Result.Success();
    }

    public async Task<Result> RemoveUserFromOrganizationAsync(
        Guid userId, Guid organizationId, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return ensureAuth;

        var response = await _httpClient.DeleteAsync(
            $"/admin/realms/{_options.Realm}/organizations/{organizationId}/members/{userId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result.NotFound($"User {userId} is not a member of organization {organizationId}.");

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult(response, "RemoveUserFromOrganization", ct);

        _logger.LogInformation("Removed user {UserId} from organization {OrgId}", userId, organizationId);
        return Result.Success();
    }

    public async Task<Result<Guid>> CreateOrganizationAsync(
        string name, Guid clinicId, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return Result<Guid>.Error(string.Join("; ", ensureAuth.Errors));

        var payload = new
        {
            name,
            attributes = new Dictionary<string, string[]>
            {
                ["clinicId"] = new[] { clinicId.ToString() }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/organizations", payload, JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return Result<Guid>.Conflict($"Organization with name '{name}' already exists.");

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult<Guid>(response, "CreateOrganization", ct);

        var locationHeader = response.Headers.Location?.ToString();
        if (locationHeader is null)
            return Result<Guid>.Error("Keycloak did not return a Location header after organization creation.");

        var orgIdString = locationHeader.Split('/').LastOrDefault();
        if (!Guid.TryParse(orgIdString, out var orgId))
            return Result<Guid>.Error($"Could not parse organization ID from Location header: {locationHeader}");

        _logger.LogInformation("Created Keycloak organization {OrgId} for clinic {ClinicId}", orgId, clinicId);
        return Result<Guid>.Success(orgId);
    }

    public async Task<Result> UpdateUserRolesAsync(
        Guid userId, Guid organizationId, IReadOnlyList<string> newRoles, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return ensureAuth;

        // Get current roles and remove them, then assign new ones
        var currentRolesResponse = await _httpClient.GetAsync(
            $"/admin/realms/{_options.Realm}/organizations/{organizationId}/members/{userId}/organization-roles", ct);

        if (!currentRolesResponse.IsSuccessStatusCode)
            return await ToErrorResult(currentRolesResponse, "GetCurrentRoles", ct);

        var currentRoles = await currentRolesResponse.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(JsonOptions, ct);

        if (currentRoles is { Count: > 0 })
        {
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete,
                $"/admin/realms/{_options.Realm}/organizations/{organizationId}/members/{userId}/organization-roles")
            {
                Content = JsonContent.Create(currentRoles, options: JsonOptions)
            };
            var deleteResponse = await _httpClient.SendAsync(deleteRequest, ct);
            if (!deleteResponse.IsSuccessStatusCode)
                return await ToErrorResult(deleteResponse, "RemoveCurrentRoles", ct);
        }

        if (newRoles.Count > 0)
        {
            var assignResult = await AssignOrganizationRolesAsync(userId, organizationId, newRoles, ct);
            if (!assignResult.IsSuccess) return assignResult;
        }

        _logger.LogInformation("Updated roles for user {UserId} in org {OrgId} to [{Roles}]",
            userId, organizationId, string.Join(", ", newRoles));
        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess) return ensureAuth;

        var response = await _httpClient.PutAsJsonAsync(
            $"/admin/realms/{_options.Realm}/users/{userId}",
            new { enabled = false },
            JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result.NotFound($"User {userId} not found in Keycloak.");

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult(response, "DeactivateUser", ct);

        _logger.LogInformation("Deactivated Keycloak user {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<KeycloakOrganizationDto>>> ListUserOrganizationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var ensureAuth = await EnsureAuthenticatedAsync(ct);
        if (!ensureAuth.IsSuccess)
            return Result<IReadOnlyList<KeycloakOrganizationDto>>.Error(string.Join("; ", ensureAuth.Errors));

        var response = await _httpClient.GetAsync(
            $"/admin/realms/{_options.Realm}/users/{userId}/organizations", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result<IReadOnlyList<KeycloakOrganizationDto>>.NotFound($"User {userId} not found.");

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult<IReadOnlyList<KeycloakOrganizationDto>>(response, "ListUserOrganizations", ct);

        var orgs = await response.Content.ReadFromJsonAsync<List<KeycloakOrganizationRepresentation>>(JsonOptions, ct);

        var dtos = (orgs ?? []).Select(o =>
        {
            var clinicId = Guid.Empty;
            if (o.Attributes?.TryGetValue("clinicId", out var clinicIdValues) == true
                && clinicIdValues.Length > 0)
            {
                Guid.TryParse(clinicIdValues[0], out clinicId);
            }
            return new KeycloakOrganizationDto(o.Id, o.Name ?? string.Empty, clinicId);
        }).ToList();

        return Result<IReadOnlyList<KeycloakOrganizationDto>>.Success(dtos);
    }

    // --- Private helpers ---

    private async Task<Result> EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (DateTimeOffset.UtcNow < _tokenExpiry)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cachedAccessToken);
            return Result.Success();
        }

        var tokenEndpoint = $"/realms/{_options.Realm}/protocol/openid-connect/token";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        var response = await _httpClient.PostAsync(tokenEndpoint, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to obtain Keycloak access token: {StatusCode} {Body}",
                response.StatusCode, body);
            return Result.Error("Failed to authenticate with Keycloak. Check client credentials.");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions, ct);
        if (tokenResponse is null)
            return Result.Error("Empty token response from Keycloak.");

        _cachedAccessToken = tokenResponse.AccessToken;
        // Expire 30 seconds early to avoid edge cases
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30);

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _cachedAccessToken);

        return Result.Success();
    }

    private async Task<Result> AssignOrganizationRolesAsync(
        Guid userId, Guid organizationId, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var roleRepresentations = roles.Select(r => new KeycloakRoleRepresentation { Name = r }).ToList();

        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_options.Realm}/organizations/{organizationId}/members/{userId}/organization-roles",
            roleRepresentations, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            return await ToErrorResult(response, "AssignOrganizationRoles", ct);

        return Result.Success();
    }

    private async Task<Result> ToErrorResult(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Keycloak {Operation} failed: {StatusCode} {Body}", operation, response.StatusCode, body);
        return Result.Error($"Keycloak {operation} failed with status {(int)response.StatusCode}: {body}");
    }

    private async Task<Result<T>> ToErrorResult<T>(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Keycloak {Operation} failed: {StatusCode} {Body}", operation, response.StatusCode, body);
        return Result<T>.Error($"Keycloak {operation} failed with status {(int)response.StatusCode}: {body}");
    }

    // --- Internal DTOs for Keycloak API responses ---

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class KeycloakRoleRepresentation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class KeycloakOrganizationRepresentation
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, string[]>? Attributes { get; set; }
    }
}
