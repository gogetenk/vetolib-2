using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListAppointments;

internal class ListAppointmentsHandler : IRequestHandler<ListAppointmentsQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public ListAppointmentsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(ListAppointmentsQuery query, CancellationToken ct)
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date == query.Date)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);

        var dtos = appointments.Select(a => a.ToDto()).ToList();

        return Result<IReadOnlyList<AppointmentDto>>.Success(dtos);
    }
}
