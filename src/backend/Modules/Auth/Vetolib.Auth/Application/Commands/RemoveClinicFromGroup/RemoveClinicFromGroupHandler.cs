using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;

internal class RemoveClinicFromGroupHandler : IRequestHandler<RemoveClinicFromGroupCommand, Result>
{
    private readonly AuthDbContext _context;

    public RemoveClinicFromGroupHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveClinicFromGroupCommand cmd, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == cmd.GroupId, ct);

        if (group is null)
            return Result.NotFound("Clinic group not found");

        // Verify ownership — prevent IDOR
        if (group.OwnerUserId != cmd.RequestingUserId)
            return Result.Forbidden();

        var removeResult = group.RemoveClinic(cmd.ClinicId);
        if (!removeResult.IsSuccess)
            return removeResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
