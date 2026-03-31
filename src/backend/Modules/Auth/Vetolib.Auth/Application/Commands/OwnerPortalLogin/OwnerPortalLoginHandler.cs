using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Services;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Commands.OwnerPortalLogin;

internal class OwnerPortalLoginHandler : IRequestHandler<OwnerPortalLoginCommand, Result<OwnerPortalTokenDto>>
{
    private readonly AuthDbContext _context;
    private readonly IOwnerPortalJwtService _jwtService;
    private readonly IOwnerAccountLinker _ownerAccountLinker;

    public OwnerPortalLoginHandler(
        AuthDbContext context,
        IOwnerPortalJwtService jwtService,
        IOwnerAccountLinker ownerAccountLinker)
    {
        _context = context;
        _jwtService = jwtService;
        _ownerAccountLinker = ownerAccountLinker;
    }

    public async Task<Result<OwnerPortalTokenDto>> Handle(OwnerPortalLoginCommand cmd, CancellationToken ct)
    {
        var normalizedEmail = cmd.Email.Trim().ToLowerInvariant();

        var account = await _context.OwnerAccounts
            .FirstOrDefaultAsync(a => a.Email == normalizedEmail, ct);

        if (account is null)
            return Result<OwnerPortalTokenDto>.Error("INVALID_CREDENTIALS:Invalid email or password");

        if (!account.VerifyPassword(cmd.Password))
            return Result<OwnerPortalTokenDto>.Error("INVALID_CREDENTIALS:Invalid email or password");

        // Get linked clinic IDs
        var linkedClinicIds = await _ownerAccountLinker.GetLinkedClinicIdsAsync(account.Id, ct);

        // Generate portal-specific JWT
        var accessToken = _jwtService.GenerateOwnerPortalToken(account, linkedClinicIds);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return Result<OwnerPortalTokenDto>.Success(
            new OwnerPortalTokenDto(accessToken, refreshToken, account.ToDto(), linkedClinicIds));
    }
}
