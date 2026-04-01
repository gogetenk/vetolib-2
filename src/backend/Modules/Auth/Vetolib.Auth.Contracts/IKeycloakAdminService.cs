using Ardalis.Result;

namespace Vetolib.Auth.Contracts;

/// <summary>
/// Wrapper around the Keycloak Admin REST API for managing users and organizations.
/// Implemented in Vetolib.Auth, registered in AuthModuleServiceRegistrar.
/// </summary>
public interface IKeycloakAdminService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
    Task<Result> AddUserToOrganizationAsync(Guid userId, Guid organizationId, IReadOnlyList<string> roles, CancellationToken ct = default);
    Task<Result> RemoveUserFromOrganizationAsync(Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<Result<Guid>> CreateOrganizationAsync(string name, Guid clinicId, CancellationToken ct = default);
    Task<Result> UpdateUserRolesAsync(Guid userId, Guid organizationId, IReadOnlyList<string> newRoles, CancellationToken ct = default);
    Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<KeycloakOrganizationDto>>> ListUserOrganizationsAsync(Guid userId, CancellationToken ct = default);
}
