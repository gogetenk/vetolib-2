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
        // Check email uniqueness globally (cross-tenant — a user can't have the same email across clinics)
        var existing = await _context.Users
            .IgnoreQueryFilters()
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
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%&*?";
        const string all = upper + lower + digits + special;

        // Guarantee at least one character from each required category
        var password = new char[PasswordRules.MinimumLength];
        password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        // Fill the rest randomly from all categories
        for (var i = 4; i < password.Length; i++)
            password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        // Shuffle to avoid predictable positions
        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
