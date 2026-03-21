using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.SwitchClinic;

internal class SwitchClinicHandler : IRequestHandler<SwitchClinicCommand, Result<AuthTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public SwitchClinicHandler(AuthDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(SwitchClinicCommand cmd, CancellationToken ct)
    {
        // Find the requesting user (cross-clinic lookup)
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == cmd.UserId && u.IsActive, ct);

        if (user is null)
            return Result<AuthTokenDto>.NotFound("User not found");

        // Check if the user has access to the target clinic via any clinic group
        var hasAccess = await _context.ClinicGroups
            .Include(g => g.Members)
            .Where(g => g.OwnerUserId == cmd.UserId)
            .AnyAsync(g => g.Members.Any(m => m.ClinicId == cmd.TargetClinicId), ct);

        // Also allow if the user's own clinic is the target
        if (!hasAccess && user.ClinicId != cmd.TargetClinicId)
            return Result<AuthTokenDto>.Forbidden();

        // Create a "virtual" user view with the target clinic to generate the JWT
        // We need a user record for the target clinic — for now we use the same user
        // but generate a token with the target ClinicId claim
        var accessToken = _jwtTokenService.GenerateAccessTokenForClinic(user, cmd.TargetClinicId);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshTokenResult = Vetolib.Auth.Application.Domain.RefreshToken.Create(user.Id, refreshTokenValue);
        if (!refreshTokenResult.IsSuccess)
            return Result<AuthTokenDto>.Error("Failed to create refresh token");

        _context.RefreshTokens.Add(refreshTokenResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<AuthTokenDto>.Success(
            new AuthTokenDto(accessToken, refreshTokenValue, user.ToDto()));
    }
}
