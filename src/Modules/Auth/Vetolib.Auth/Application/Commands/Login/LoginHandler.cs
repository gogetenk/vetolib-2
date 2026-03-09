using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.Login;

internal class LoginHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(AuthDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // Login needs to find users across all clinics, so we use IgnoreQueryFilters
        // This is acceptable because login is a pre-authentication endpoint
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (user is null)
            return Result<AuthTokenDto>.Error("INVALID_CREDENTIALS:Email ou mot de passe incorrect");

        // Check if account is locked
        if (user.IsCurrentlyLocked())
        {
            return Result<AuthTokenDto>.Error("ACCOUNT_LOCKED:Le compte est verrouille pour 15 minutes suite a des tentatives echouees");
        }

        // If lock has expired, unlock the account
        if (user.IsLocked && !user.IsCurrentlyLocked())
        {
            user.Unlock();
        }

        // Verify password
        if (!user.VerifyPassword(cmd.Password))
        {
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(ct);

            if (user.IsCurrentlyLocked())
            {
                return Result<AuthTokenDto>.Error("ACCOUNT_LOCKED:Le compte est verrouille pour 15 minutes suite a des tentatives echouees");
            }

            return Result<AuthTokenDto>.Error("INVALID_CREDENTIALS:Email ou mot de passe incorrect");
        }

        // Success: reset failed attempts
        user.ResetFailedAttempts();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenResult = Domain.RefreshToken.Create(user.Id, refreshTokenValue);
        if (!refreshTokenResult.IsSuccess)
            return Result<AuthTokenDto>.Error("Failed to create refresh token");

        _context.RefreshTokens.Add(refreshTokenResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<AuthTokenDto>.Success(
            new AuthTokenDto(accessToken, refreshTokenValue, user.ToDto()));
    }
}
