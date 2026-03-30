using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.RefreshToken;

internal class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenHandler(AuthDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        // Find the refresh token (no tenant filter on RefreshToken since it's not IMultiTenant)
        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == cmd.RefreshToken, ct);

        if (existingToken is null || !existingToken.IsValid())
            return Result<AuthTokenDto>.Error("INVALID_REFRESH_TOKEN:Le refresh token est invalide ou expire");

        // Revoke the old token (rotation)
        existingToken.Revoke();

        // Find the user
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, ct);

        if (user is null)
            return Result<AuthTokenDto>.Error("INVALID_REFRESH_TOKEN:Utilisateur non trouve");

        // Check if user account is deactivated — persist revocation and reject
        if (!user.IsActive)
        {
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Token was already rotated by another request — safe to ignore
            }
            return Result<AuthTokenDto>.Error("ACCOUNT_DEACTIVATED:Your account has been deactivated. Contact your clinic admin.");
        }

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenResult = Domain.RefreshToken.Create(user.Id, newRefreshTokenValue);
        if (!newRefreshTokenResult.IsSuccess)
            return Result<AuthTokenDto>.Error("Failed to create new refresh token");

        _context.RefreshTokens.Add(newRefreshTokenResult.Value);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Race condition: another request already rotated this token.
            // The concurrency token on IsRevoked ensures only one request wins.
            return Result<AuthTokenDto>.Error("INVALID_REFRESH_TOKEN:Token has already been used");
        }

        return Result<AuthTokenDto>.Success(
            new AuthTokenDto(newAccessToken, newRefreshTokenValue, user.ToDto()));
    }
}
