using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.DeactivateUser;

internal class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly AuthDbContext _context;
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly ILogger<DeactivateUserHandler> _logger;

    public DeactivateUserHandler(
        AuthDbContext context,
        IKeycloakAdminService keycloakAdmin,
        ILogger<DeactivateUserHandler> logger)
    {
        _context = context;
        _keycloakAdmin = keycloakAdmin;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateUserCommand cmd, CancellationToken ct)
    {
        // Admin cannot deactivate themselves
        if (cmd.RequestingUserId == cmd.TargetUserId)
            return Result.Invalid(new List<ValidationError>
            {
                new("targetUserId", "Cannot deactivate your own account")
            });

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == cmd.TargetUserId, ct);

        if (targetUser is null)
            return Result.NotFound($"User '{cmd.TargetUserId}' not found.");

        targetUser.Deactivate();
        await _context.SaveChangesAsync(ct);

        // Sync deactivation to Keycloak (best-effort — DB is source of truth)
        if (targetUser.KeycloakUserId.HasValue)
        {
            var kcResult = await _keycloakAdmin.DeactivateUserAsync(targetUser.KeycloakUserId.Value, ct);
            if (!kcResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to deactivate user {UserId} in Keycloak (KeycloakId={KeycloakUserId}): {Errors}",
                    targetUser.Id, targetUser.KeycloakUserId.Value, string.Join("; ", kcResult.Errors));
            }
        }

        return Result.Success();
    }
}
