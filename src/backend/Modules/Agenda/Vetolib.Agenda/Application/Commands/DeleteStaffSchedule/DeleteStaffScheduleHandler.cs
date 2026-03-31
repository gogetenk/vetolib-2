using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.DeleteStaffSchedule;

internal class DeleteStaffScheduleHandler : IRequestHandler<DeleteStaffScheduleCommand, Result>
{
    private readonly AgendaDbContext _context;

    public DeleteStaffScheduleHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteStaffScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await _context.StaffSchedules
            .FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);

        if (schedule is null)
            return Result.NotFound("SCHEDULE_NOT_FOUND:Staff schedule entry not found");

        _context.StaffSchedules.Remove(schedule);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
