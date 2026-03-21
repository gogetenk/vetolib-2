using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Infrastructure;

namespace Vetolib.Auth.Application.Commands.AddClinicToGroup;

internal class AddClinicToGroupHandler : IRequestHandler<AddClinicToGroupCommand, Result>
{
    private readonly AuthDbContext _context;

    public AddClinicToGroupHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AddClinicToGroupCommand cmd, CancellationToken ct)
    {
        var group = await _context.ClinicGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == cmd.GroupId, ct);

        if (group is null)
            return Result.NotFound("Clinic group not found");

        // Verify the clinic exists
        var clinicExists = await _context.Clinics
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == cmd.ClinicId, ct);

        if (!clinicExists)
            return Result.NotFound("Clinic not found");

        var addResult = group.AddClinic(cmd.ClinicId);
        if (!addResult.IsSuccess)
            return addResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
