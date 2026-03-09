using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.DeactivateUser;

internal class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly AuthDbContext _context;

    public DeactivateUserHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateUserCommand cmd, CancellationToken ct)
    {
        // Admin cannot deactivate themselves
        if (cmd.RequestingUserId == cmd.TargetUserId)
            return Result.Invalid(new List<ValidationError>
            {
                new("targetUserId", "Cannot deactivate your own account")
            });

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == cmd.TargetUserId, ct);

        if (targetUser is null)
            return Result.NotFound($"User '{cmd.TargetUserId}' not found.");

        targetUser.Deactivate();
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
