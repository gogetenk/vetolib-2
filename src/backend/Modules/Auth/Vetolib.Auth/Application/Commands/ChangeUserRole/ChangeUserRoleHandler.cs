using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.ChangeUserRole;

internal class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, Result>
{
    private readonly AuthDbContext _context;

    public ChangeUserRoleHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeUserRoleCommand cmd, CancellationToken ct)
    {
        // Check if requesting user is trying to change their own role (ADMIN cannot demote themselves)
        if (cmd.RequestingUserId == cmd.TargetUserId)
        {
            var requestingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == cmd.RequestingUserId, ct);

            if (requestingUser?.Role == UserRole.Admin)
                return Result.Error("Cannot change your own admin role.");
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == cmd.TargetUserId, ct);

        if (targetUser is null)
            return Result.NotFound($"User '{cmd.TargetUserId}' not found.");

        targetUser.ChangeRole(cmd.NewRole);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
