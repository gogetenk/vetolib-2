using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using DomainRefreshToken = Vetolib.Auth.Application.Domain.RefreshToken;

namespace Vetolib.Auth.Application.Commands.RegisterClinic;

internal class RegisterClinicHandler : IRequestHandler<RegisterClinicCommand, Result<RegisterClinicResponse>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthSecurityOptions _securityOptions;
    private readonly IKeycloakAdminService? _keycloakAdminService;
    private readonly ILogger<RegisterClinicHandler> _logger;

    public RegisterClinicHandler(
        AuthDbContext context,
        IJwtTokenService jwtTokenService,
        IOptions<AuthSecurityOptions> securityOptions,
        ILogger<RegisterClinicHandler> logger,
        IKeycloakAdminService? keycloakAdminService = null)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _securityOptions = securityOptions.Value;
        _keycloakAdminService = keycloakAdminService;
        _logger = logger;
    }

    public async Task<Result<RegisterClinicResponse>> Handle(RegisterClinicCommand cmd, CancellationToken ct)
    {
        // Cross-tenant check: email must be globally unique
        var emailExists = await _context.Users
            .IgnoreQueryFilters() // EXCEPTION: registration is pre-tenant — cross-tenant email uniqueness check
            .AnyAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (emailExists)
            return Result<RegisterClinicResponse>.Error("EMAIL_EXISTS:This email is already registered");

        // Create the clinic (new tenant)
        var clinicResult = Clinic.Create(cmd.ClinicName, _securityOptions.TrialDays);
        if (!clinicResult.IsSuccess)
            return clinicResult.Map(_ => (RegisterClinicResponse)null!);

        var clinic = clinicResult.Value;

        // Create the admin user for this clinic
        var userResult = User.Create(clinic.Id, cmd.Email, cmd.Password, UserRole.Admin);
        if (!userResult.IsSuccess)
            return userResult.Map(_ => (RegisterClinicResponse)null!);

        var user = userResult.Value;

        // Process referral code if provided
        if (!string.IsNullOrWhiteSpace(cmd.ReferralCode))
        {
            var referralCode = await _context.ReferralCodes
                .FirstOrDefaultAsync(r => r.Code == cmd.ReferralCode.ToUpperInvariant(), ct);

            if (referralCode is not null)
            {
                user.SetReferrer(referralCode.OwnerUserId);
                referralCode.IncrementUsage();
            }
            // If referral code not found, we silently ignore it (don't block registration)
        }

        // Persist both in the same transaction
        _context.Clinics.Add(clinic);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        // Sync with Keycloak (best-effort — registration succeeds even if Keycloak is unavailable)
        await SyncWithKeycloakAsync(clinic, user, cmd.Password, ct);

        // Generate tokens for immediate login
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenResult = DomainRefreshToken.Create(user.Id, refreshTokenValue);
        if (!refreshTokenResult.IsSuccess)
            return Result<RegisterClinicResponse>.Error("Failed to create refresh token");

        _context.RefreshTokens.Add(refreshTokenResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<RegisterClinicResponse>.Success(new RegisterClinicResponse(
            accessToken,
            refreshTokenValue,
            clinic.Id,
            clinic.Name,
            user.ToDto()));
    }

    private async Task SyncWithKeycloakAsync(Clinic clinic, User user, string password, CancellationToken ct)
    {
        if (_keycloakAdminService is null)
            return;

        try
        {
            // 1. Create the Keycloak Organization for this clinic
            var orgResult = await _keycloakAdminService.CreateOrganizationAsync(clinic.Name, clinic.Id, ct);
            if (!orgResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create Keycloak organization for clinic {ClinicId}: {Errors}",
                    clinic.Id, string.Join(", ", orgResult.Errors));
                return;
            }

            var kcOrgId = orgResult.Value;
            clinic.SetKeycloakOrganizationId(kcOrgId);

            // 2. Create the Keycloak user
            var userResult = await _keycloakAdminService.CreateUserAsync(user.Email, password, string.Empty, string.Empty, ct);
            if (!userResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create Keycloak user for {Email}: {Errors}",
                    user.Email, string.Join(", ", userResult.Errors));
                return;
            }

            var kcUserId = userResult.Value;
            user.SetKeycloakUserId(kcUserId);

            // 3. Add user to organization with Admin role
            var addResult = await _keycloakAdminService.AddUserToOrganizationAsync(kcUserId, kcOrgId, new[] { "Admin" }, ct);
            if (!addResult.IsSuccess)
            {
                _logger.LogWarning("Failed to add user {KcUserId} to organization {KcOrgId}: {Errors}",
                    kcUserId, kcOrgId, string.Join(", ", addResult.Errors));
            }

            // Persist Keycloak IDs
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Keycloak sync failed during clinic registration for clinic {ClinicId}. Registration will proceed without Keycloak sync.", clinic.Id);
        }
    }
}
