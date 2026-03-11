using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListOwnerAppointments;

internal class ListOwnerAppointmentsHandler
    : IRequestHandler<ListOwnerAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public ListOwnerAppointmentsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(
        ListOwnerAppointmentsQuery query,
        CancellationToken ct)
    {
        // IgnoreQueryFilters() is intentional: the clinic context is not populated for portal requests.
        // We explicitly filter by both OwnerId and ClinicId for data isolation.
        var appointments = await _context.Appointments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.ClinicId == query.ClinicId
                        && a.OwnerId == query.OwnerId)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.StartTime)
            .Select(a => a.ToDto())
            .ToListAsync(ct);

        return Result<List<AppointmentDto>>.Success(appointments);
    }
}
