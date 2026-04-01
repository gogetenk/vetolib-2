using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.ChangeUserRole;

internal class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly ILogger<ChangeUserRoleHandler> _logger;

    public ChangeUserRoleHandler(
        AuthDbContext context,
        IKeycloakAdminService keycloakAdmin,
        ILogger<ChangeUserRoleHandler> logger)
    {
        _context = context;
        _keycloakAdmin = keycloakAdmin;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeUserRoleCommand cmd, CancellationToken ct)
    {
        // Check if requesting user is trying to change their own role (ADMIN cannot demote themselves)
        if (cmd.RequestingUserId == cmd.TargetUserId)
        {
            var requestingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == cmd.RequestingUserId, ct);

            if (requestingUser?.Role == UserRole.Admin)
                return Result.Error("Cannot change your own admin role.");
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == cmd.TargetUserId, ct);

        if (targetUser is null)
            return Result.NotFound($"User '{cmd.TargetUserId}' not found.");

        targetUser.ChangeRole(cmd.NewRole);
        await _context.SaveChangesAsync(ct);

        // Sync role change to Keycloak (best-effort — DB is source of truth)
        if (targetUser.KeycloakUserId.HasValue)
        {
            var orgsResult = await _keycloakAdmin.ListUserOrganizationsAsync(targetUser.KeycloakUserId.Value, ct);
            if (!orgsResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to list Keycloak organizations for user {UserId} (KeycloakId={KeycloakUserId}): {Errors}",
                    targetUser.Id, targetUser.KeycloakUserId.Value, string.Join("; ", orgsResult.Errors));
                return Result.Success();
            }

            var org = orgsResult.Value.FirstOrDefault(o => o.ClinicId == targetUser.ClinicId);
            if (org is null)
            {
                _logger.LogWarning(
                    "No Keycloak organization found for clinic {ClinicId} when updating roles for user {UserId}",
                    targetUser.ClinicId, targetUser.Id);
                return Result.Success();
            }

            var rolesResult = await _keycloakAdmin.UpdateUserRolesAsync(
                targetUser.KeycloakUserId.Value,
                org.Id,
                [cmd.NewRole.ToString()],
                ct);

            if (!rolesResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to update roles in Keycloak for user {UserId} (KeycloakId={KeycloakUserId}): {Errors}",
                    targetUser.Id, targetUser.KeycloakUserId.Value, string.Join("; ", rolesResult.Errors));
            }
        }

        return Result.Success();
    }
}
