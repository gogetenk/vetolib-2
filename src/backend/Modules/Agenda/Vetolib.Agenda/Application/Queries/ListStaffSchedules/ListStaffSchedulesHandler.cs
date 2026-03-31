using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListStaffSchedules;

internal class ListStaffSchedulesHandler : IRequestHandler<ListStaffSchedulesQuery, Result<List<StaffScheduleDto>>>
{
    private readonly AgendaDbContext _context;

    public ListStaffSchedulesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StaffScheduleDto>>> Handle(ListStaffSchedulesQuery query, CancellationToken ct)
    {
        if (query.From > query.To)
            return Result<List<StaffScheduleDto>>.Error("INVALID_DATE_RANGE:'from' date must be before or equal to 'to' date");

        var schedules = await _context.StaffSchedules
            .AsNoTracking()
            .Where(s => s.Date >= query.From && s.Date <= query.To)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        var dtos = schedules.Select(s => s.ToDto()).ToList();
        return Result<List<StaffScheduleDto>>.Success(dtos);
    }
}
