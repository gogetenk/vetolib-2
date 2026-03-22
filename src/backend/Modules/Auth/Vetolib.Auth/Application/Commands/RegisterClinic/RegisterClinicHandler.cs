using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public RegisterClinicHandler(AuthDbContext context, IJwtTokenService jwtTokenService, IOptions<AuthSecurityOptions> securityOptions)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _securityOptions = securityOptions.Value;
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

        // Persist both in the same transaction
        _context.Clinics.Add(clinic);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

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
}
