using System.Security.Cryptography;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.InviteUser;

internal class InviteUserHandler : IRequestHandler<InviteUserCommand, Result<InviteUserResponse>>
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;

    public InviteUserHandler(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<InviteUserResponse>> Handle(InviteUserCommand cmd, CancellationToken ct)
    {
        // Check email uniqueness within clinic
        var existing = await _context.Users
            .AnyAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);

        if (existing)
            return Result<InviteUserResponse>.Error($"A user with email '{cmd.Email}' already exists in this clinic.");

        // Generate temporary password (8 chars alphanumeric)
        var temporaryPassword = GenerateTemporaryPassword();

        // Resolve clinic name for the invitation email
        var clinicName = _configuration["ClinicName"] ?? "Desert Paws Veterinary Clinic";

        // Create user via domain factory — domain event is added inside User.Invite()
        var userResult = User.Invite(cmd.ClinicId, cmd.Email, cmd.FullName, temporaryPassword, cmd.Role, clinicName);
        if (!userResult.IsSuccess)
            return Result<InviteUserResponse>.Invalid(userResult.ValidationErrors.ToList());

        _context.Users.Add(userResult.Value);

        // SaveChangesAsync will dispatch the UserInvitedDomainEvent → publishes UserInvitedIntegrationEvent
        await _context.SaveChangesAsync(ct);

        return Result<InviteUserResponse>.Success(new InviteUserResponse(
            userResult.Value.Id,
            userResult.Value.Email,
            userResult.Value.FullName,
            userResult.Value.Role));
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }
}
