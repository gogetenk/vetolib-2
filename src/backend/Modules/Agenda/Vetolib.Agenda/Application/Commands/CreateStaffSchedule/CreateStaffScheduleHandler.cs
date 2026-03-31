using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateStaffSchedule;

internal class CreateStaffScheduleHandler : IRequestHandler<CreateStaffScheduleCommand, Result<StaffScheduleDto>>
{
    private readonly AgendaDbContext _context;

    public CreateStaffScheduleHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffScheduleDto>> Handle(CreateStaffScheduleCommand cmd, CancellationToken ct)
    {
        // Check for overlapping schedule for the same user on the same date
        var overlapping = await _context.StaffSchedules
            .AnyAsync(s => s.UserId == cmd.UserId
                        && s.Date == cmd.Date
                        && s.StartTime < cmd.EndTime
                        && s.EndTime > cmd.StartTime, ct);

        if (overlapping)
        {
            return Result<StaffScheduleDto>.Error(
                "SCHEDULE_OVERLAP:This user already has a schedule entry that overlaps with the requested time range");
        }

        var result = StaffSchedule.Create(
            cmd.ClinicId,
            cmd.UserId,
            cmd.UserName,
            cmd.Date,
            cmd.StartTime,
            cmd.EndTime,
            cmd.ShiftType,
            cmd.IsAvailable);

        if (!result.IsSuccess)
            return Result<StaffScheduleDto>.Invalid(result.ValidationErrors.ToList());

        _context.StaffSchedules.Add(result.Value);
        await _context.SaveChangesAsync(ct);

        return Result<StaffScheduleDto>.Success(result.Value.ToDto());
    }
}
