using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.Login;

internal class LoginHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthSecurityOptions _securityOptions;

    public LoginHandler(AuthDbContext context, IJwtTokenService jwtTokenService, IOptions<AuthSecurityOptions> securityOptions)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _securityOptions = securityOptions.Value;
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
        if (!user.VerifyPassword(cmd.Password))
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
}
