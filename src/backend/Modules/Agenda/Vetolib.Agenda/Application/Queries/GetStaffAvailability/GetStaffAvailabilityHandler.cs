using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetStaffAvailability;

internal class GetStaffAvailabilityHandler : IRequestHandler<GetStaffAvailabilityQuery, Result<List<StaffScheduleDto>>>
{
    private readonly AgendaDbContext _context;

    public GetStaffAvailabilityHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StaffScheduleDto>>> Handle(GetStaffAvailabilityQuery query, CancellationToken ct)
    {
        var schedules = await _context.StaffSchedules
            .AsNoTracking()
            .Where(s => s.Date == query.Date && s.IsAvailable)
            .OrderBy(s => s.UserName)
            .ThenBy(s => s.StartTime)
            .ToListAsync(ct);

        var dtos = schedules.Select(s => s.ToDto()).ToList();
        return Result<List<StaffScheduleDto>>.Success(dtos);
    }
}
