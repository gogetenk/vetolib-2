using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.Login;

internal class LoginHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthSecurityOptions _securityOptions;
    private readonly IKeycloakAdminService _keycloakAdminService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        AuthDbContext context,
        IJwtTokenService jwtTokenService,
        IOptions<AuthSecurityOptions> securityOptions,
        IKeycloakAdminService keycloakAdminService,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _securityOptions = securityOptions.Value;
        _keycloakAdminService = keycloakAdminService;
        _logger = logger;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // Login needs to find users across all clinics, so we use IgnoreQueryFilters
        // This is acceptable because login is a pre-authentication endpoint
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (user is null)
            return Result<AuthTokenDto>.Error("INVALID_CREDENTIALS:Invalid email or password");

        // Check if account is deactivated
        if (!user.IsActive)
            return Result<AuthTokenDto>.Error("ACCOUNT_DEACTIVATED:Your account has been deactivated. Contact your clinic admin.");

        // Check if account is locked
        if (user.IsCurrentlyLocked())
        {
            return Result<AuthTokenDto>.Error("ACCOUNT_LOCKED:Account is locked for 15 minutes due to failed login attempts");
        }

        // If lock has expired, unlock the account
        if (user.IsLocked && !user.IsCurrentlyLocked())
        {
            user.Unlock();
        }

        // Verify password
        var verifyResult = user.VerifyPassword(cmd.Password);
        if (!verifyResult.IsSuccess)
        {
            // Keycloak-only users cannot login via local password
            return Result<AuthTokenDto>.Error(verifyResult.Errors.First());
        }

        if (!verifyResult.Value)
        {
            user.RecordFailedLogin(_securityOptions.MaxFailedLoginAttempts, _securityOptions.LockoutMinutes);
            await _context.SaveChangesAsync(ct);

            if (user.IsCurrentlyLocked())
            {
                return Result<AuthTokenDto>.Error("ACCOUNT_LOCKED:Account is locked for 15 minutes due to failed login attempts");
            }

            return Result<AuthTokenDto>.Error("INVALID_CREDENTIALS:Invalid email or password");
        }

        // Success: reset failed attempts
        user.ResetFailedAttempts();

        // Lazy migration: if Legacy user without Keycloak, create them in Keycloak
        await TryMigrateToKeycloakAsync(user, cmd.Password, ct);

        // Check if user must change their password before accessing the app
        if (user.MustChangePassword)
        {
            await _context.SaveChangesAsync(ct);
            return Result<AuthTokenDto>.Error("MUST_CHANGE_PASSWORD:You must change your password before accessing the application");
        }

        // Check email verification status — allow login but include warning
        var emailNotVerified = !user.EmailVerified;

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenResult = Domain.RefreshToken.Create(user.Id, refreshTokenValue);
        if (!refreshTokenResult.IsSuccess)
            return Result<AuthTokenDto>.Error("Failed to create refresh token");

        _context.RefreshTokens.Add(refreshTokenResult.Value);
        await _context.SaveChangesAsync(ct);

        var tokenDto = new AuthTokenDto(accessToken, refreshTokenValue, user.ToDto());

        if (emailNotVerified)
        {
            // Return success with a message — login is allowed but UI should prompt verification
            return Result<AuthTokenDto>.Success(tokenDto, "EMAIL_NOT_VERIFIED:Please verify your email address");
        }

        return Result<AuthTokenDto>.Success(tokenDto);
    }

    /// <summary>
    /// Best-effort lazy migration: creates the user in Keycloak and adds them to their clinic's organization.
    /// If anything fails, the login proceeds normally — migration will be retried on next login.
    /// </summary>
    private async Task TryMigrateToKeycloakAsync(User user, string plaintextPassword, CancellationToken ct)
    {
        if (user.AuthProvider != AuthProvider.Legacy || user.KeycloakUserId is not null)
            return;

        try
        {
            // Split FullName into first/last for Keycloak — use email prefix as fallback
            var (firstName, lastName) = SplitName(user.FullName, user.Email);

            var createResult = await _keycloakAdminService.CreateUserAsync(
                user.Email, plaintextPassword, firstName, lastName, ct);

            if (!createResult.IsSuccess)
            {
                _logger.LogWarning("Lazy Keycloak migration failed for user {UserId}: CreateUser returned {Errors}",
                    user.Id, string.Join(", ", createResult.Errors));
                return;
            }

            var keycloakUserId = createResult.Value;

            // Try to add the user to their clinic's Keycloak organization
            var clinic = await _context.Clinics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == user.ClinicId, ct);

            if (clinic?.KeycloakOrganizationId is not null)
            {
                var roleStr = user.Role.ToString();
                var addOrgResult = await _keycloakAdminService.AddUserToOrganizationAsync(
                    keycloakUserId, clinic.KeycloakOrganizationId.Value, new[] { roleStr }, ct);

                if (!addOrgResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Lazy Keycloak migration: user {UserId} created in Keycloak ({KeycloakUserId}) but failed to add to organization {OrgId}: {Errors}",
                        user.Id, keycloakUserId, clinic.KeycloakOrganizationId.Value,
                        string.Join(", ", addOrgResult.Errors));
                }
            }

            // Mark user as migrated
            var migrateResult = user.MigrateToKeycloak(keycloakUserId);
            if (!migrateResult.IsSuccess)
            {
                _logger.LogWarning("Lazy Keycloak migration: domain MigrateToKeycloak failed for user {UserId}: {Errors}",
                    user.Id, string.Join(", ", migrateResult.Errors));
                return;
            }

            _logger.LogInformation("Lazy Keycloak migration completed for user {UserId} → Keycloak {KeycloakUserId}",
                user.Id, keycloakUserId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lazy Keycloak migration failed with exception for user {UserId}", user.Id);
        }
    }

    private static (string FirstName, string LastName) SplitName(string fullName, string email)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? (parts[0], parts[1]) : (parts[0], parts[0]);
        }

        // Fallback: use email local part
        var localPart = email.Split('@')[0];
        return (localPart, localPart);
    }
}
