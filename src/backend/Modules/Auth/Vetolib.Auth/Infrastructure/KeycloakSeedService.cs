using Ardalis.Result;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Infrastructure;

/// <summary>
/// Seeds demo organizations and users in Keycloak on startup.
/// Only runs in Development environment. Idempotent — safe to call on every restart.
/// </summary>
internal sealed class KeycloakSeedService : IHostedService
{
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<KeycloakSeedService> _logger;

    // Deterministic IDs matching DbInitializer seed data.
    private static readonly Guid DubaiClinicId = new("00000000-0000-0000-0001-000000000001");
    private static readonly Guid AbuDhabiClinicId = new("00000000-0000-0000-0001-000000000002");

    // Demo users — passwords are for local dev only.
    private const string DefaultDevPassword = "Vetolib2026!";

    public KeycloakSeedService(
        IKeycloakAdminService keycloakAdmin,
        IHostEnvironment environment,
        ILogger<KeycloakSeedService> logger)
    {
        _keycloakAdmin = keycloakAdmin;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogDebug("Keycloak seed: skipping — not in Development environment.");
            return;
        }

        _logger.LogInformation("Keycloak seed: starting demo data provisioning...");

        try
        {
            // Step 1: Create organizations
            var dubaiOrgId = await CreateOrganizationIfNotExistsAsync(
                "Dubai Pet Clinic", DubaiClinicId, cancellationToken);
            var abuDhabiOrgId = await CreateOrganizationIfNotExistsAsync(
                "Abu Dhabi Animal Hospital", AbuDhabiClinicId, cancellationToken);

            if (dubaiOrgId is null && abuDhabiOrgId is null)
            {
                _logger.LogInformation("Keycloak seed: could not create any organizations — skipping user creation.");
                return;
            }

            // Step 2: Create users and assign to organizations
            var adminUserId = await CreateUserIfNotExistsAsync(
                "admin@vetolib.ae", DefaultDevPassword, "Omar", "Al-Rashid", cancellationToken);

            var vetUserId = await CreateUserIfNotExistsAsync(
                "vet@vetolib.ae", DefaultDevPassword, "Sarah", "Johnson", cancellationToken);

            var receptionUserId = await CreateUserIfNotExistsAsync(
                "reception@vetolib.ae", DefaultDevPassword, "Fatima", "Hassan", cancellationToken);

            // Step 3: Assign users to organizations with roles
            if (adminUserId is not null)
            {
                if (dubaiOrgId is not null)
                    await AddUserToOrgSafeAsync(adminUserId.Value, dubaiOrgId.Value, ["Admin"], cancellationToken);
                if (abuDhabiOrgId is not null)
                    await AddUserToOrgSafeAsync(adminUserId.Value, abuDhabiOrgId.Value, ["Admin"], cancellationToken);
            }

            if (vetUserId is not null && dubaiOrgId is not null)
                await AddUserToOrgSafeAsync(vetUserId.Value, dubaiOrgId.Value, ["Vet"], cancellationToken);

            if (receptionUserId is not null && dubaiOrgId is not null)
                await AddUserToOrgSafeAsync(receptionUserId.Value, dubaiOrgId.Value, ["Receptionist"], cancellationToken);

            _logger.LogInformation("Keycloak seed: demo data provisioning complete.");
        }
        catch (Exception ex)
        {
            // Seed failure must not crash the application — log and continue.
            _logger.LogError(ex, "Keycloak seed: failed to provision demo data. The application will continue without seeded Keycloak data.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Creates an organization if it doesn't already exist.
    /// Returns the organization ID (from Keycloak) or null if creation failed.
    /// </summary>
    private async Task<Guid?> CreateOrganizationIfNotExistsAsync(
        string name, Guid clinicId, CancellationToken ct)
    {
        var result = await _keycloakAdmin.CreateOrganizationAsync(name, clinicId, ct);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Keycloak seed: created organization '{Name}' (clinicId={ClinicId}, orgId={OrgId}).",
                name, clinicId, result.Value);
            return result.Value;
        }

        // Conflict = already exists — this is expected on restart.
        if (result.Status == ResultStatus.Conflict)
        {
            _logger.LogDebug("Keycloak seed: organization '{Name}' already exists — skipping.", name);
            // Return clinicId as fallback — the org exists, we just don't have its Keycloak ID.
            // For AddUserToOrganization, the service uses clinicId-based lookup internally.
            return clinicId;
        }

        _logger.LogWarning("Keycloak seed: failed to create organization '{Name}': {Errors}",
            name, string.Join(", ", result.Errors));
        return null;
    }

    /// <summary>
    /// Creates a user if they don't already exist.
    /// Returns the user ID or null if creation failed.
    /// </summary>
    private async Task<Guid?> CreateUserIfNotExistsAsync(
        string email, string password, string firstName, string lastName, CancellationToken ct)
    {
        var result = await _keycloakAdmin.CreateUserAsync(email, password, firstName, lastName, ct);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Keycloak seed: created user '{Email}' (id={UserId}).", email, result.Value);
            return result.Value;
        }

        if (result.Status == ResultStatus.Conflict)
        {
            _logger.LogDebug("Keycloak seed: user '{Email}' already exists — skipping.", email);
            // We don't have the existing user's ID, but we need it for org assignment.
            // Return a deterministic GUID based on email so the caller has something.
            // The actual AddUserToOrganization will handle lookup by email internally.
            return null;
        }

        _logger.LogWarning("Keycloak seed: failed to create user '{Email}': {Errors}",
            email, string.Join(", ", result.Errors));
        return null;
    }

    /// <summary>
    /// Adds a user to an organization, ignoring Conflict results (already a member).
    /// </summary>
    private async Task AddUserToOrgSafeAsync(
        Guid userId, Guid organizationId, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var result = await _keycloakAdmin.AddUserToOrganizationAsync(userId, organizationId, roles, ct);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Keycloak seed: added user {UserId} to org {OrgId} with roles [{Roles}].",
                userId, organizationId, string.Join(", ", roles));
            return;
        }

        if (result.Status == ResultStatus.Conflict)
        {
            _logger.LogDebug("Keycloak seed: user {UserId} already in org {OrgId} — skipping.", userId, organizationId);
            return;
        }

        _logger.LogWarning("Keycloak seed: failed to add user {UserId} to org {OrgId}: {Errors}",
            userId, organizationId, string.Join(", ", result.Errors));
    }
}
