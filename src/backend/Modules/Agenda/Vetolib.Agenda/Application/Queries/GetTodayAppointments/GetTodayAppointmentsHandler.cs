using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetTodayAppointments;

internal class GetTodayAppointmentsHandler
    : IRequestHandler<GetTodayAppointmentsQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public GetTodayAppointmentsHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(
        GetTodayAppointmentsQuery query, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Date == today)
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);

        IReadOnlyList<AppointmentDto> dtos = appointments.Select(a => a.ToDto()).ToList();
        return Result<IReadOnlyList<AppointmentDto>>.Success(dtos);
    }
}
