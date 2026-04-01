using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.CreateUser;

internal class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly AuthDbContext _context;
    private readonly IKeycloakAdminService? _keycloakAdmin;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        AuthDbContext context,
        ILogger<CreateUserHandler> logger,
        IKeycloakAdminService? keycloakAdmin = null)
    {
        _context = context;
        _logger = logger;
        _keycloakAdmin = keycloakAdmin;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        // Check if email already exists globally (cross-tenant uniqueness)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (existingUser is not null)
            return Result<UserDto>.Error("EMAIL_EXISTS:This email is already in use");

        // Create user via domain factory
        var userResult = User.Create(cmd.ClinicId, cmd.Email, cmd.Password, cmd.Role, cmd.VetLicenseNumber);

        if (!userResult.IsSuccess)
        {
            // Check for VET_LICENSE_REQUIRED
            var vetLicenseError = userResult.ValidationErrors
                .FirstOrDefault(e => e.Identifier == "vetLicenseNumber");
            if (vetLicenseError is not null)
            {
                return Result<UserDto>.Error($"VET_LICENSE_REQUIRED:{vetLicenseError.ErrorMessage}");
            }

            // Return validation errors
            return Result<UserDto>.Invalid(userResult.ValidationErrors.ToList());
        }

        _context.Users.Add(userResult.Value);
        await _context.SaveChangesAsync(ct);

        // Sync with Keycloak (non-blocking — log warning on failure)
        await SyncWithKeycloakAsync(userResult.Value, cmd, ct);

        return Result<UserDto>.Success(userResult.Value.ToDto());
    }

    private async Task SyncWithKeycloakAsync(User user, CreateUserCommand cmd, CancellationToken ct)
    {
        if (_keycloakAdmin is null)
        {
            _logger.LogWarning("IKeycloakAdminService is not registered — skipping Keycloak sync for user {UserId}", user.Id);
            return;
        }

        try
        {
            // 1. Create user in Keycloak
            var kcResult = await _keycloakAdmin.CreateUserAsync(
                user.Email, cmd.Password, string.Empty, string.Empty, ct);

            if (!kcResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create Keycloak user for {Email}: {Errors}",
                    user.Email, string.Join("; ", kcResult.Errors));
                return;
            }

            var kcUserId = kcResult.Value;

            // Store KeycloakUserId on the User entity
            user.SetKeycloakUserId(kcUserId);
            await _context.SaveChangesAsync(ct);

            // 2. Add user to the clinic organization
            var orgResult = await _keycloakAdmin.AddUserToOrganizationAsync(
                kcUserId, cmd.ClinicId, new[] { cmd.Role.ToString() }, ct);

            if (!orgResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Keycloak user {KcUserId} created but failed to add to organization {ClinicId}: {Errors}",
                    kcUserId, cmd.ClinicId, string.Join("; ", orgResult.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Keycloak sync failed for user {UserId} — user was saved to DB successfully", user.Id);
        }
    }
}
