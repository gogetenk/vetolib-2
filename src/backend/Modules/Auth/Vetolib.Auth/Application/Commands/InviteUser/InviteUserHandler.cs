using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.InviteUser;

internal class InviteUserHandler : IRequestHandler<InviteUserCommand, Result<InviteUserResponse>>
{
    private readonly AuthDbContext _context;

    public InviteUserHandler(AuthDbContext context)
    {
        _context = context;
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

        // Create user via domain factory
        var userResult = User.Invite(cmd.ClinicId, cmd.Email, cmd.FullName, temporaryPassword, cmd.Role);
        if (!userResult.IsSuccess)
            return Result<InviteUserResponse>.Invalid(userResult.ValidationErrors.ToList());

        _context.Users.Add(userResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<InviteUserResponse>.Success(new InviteUserResponse(
            userResult.Value.Id,
            userResult.Value.Email,
            userResult.Value.FullName,
            userResult.Value.Role,
            temporaryPassword));
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
