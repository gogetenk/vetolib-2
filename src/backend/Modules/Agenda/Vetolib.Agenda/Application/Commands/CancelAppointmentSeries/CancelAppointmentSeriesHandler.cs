using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CancelAppointmentSeries;

internal class CancelAppointmentSeriesHandler : IRequestHandler<CancelAppointmentSeriesCommand, Result<int>>
{
    private readonly AgendaDbContext _context;

    public CancelAppointmentSeriesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CancelAppointmentSeriesCommand cmd, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var futureAppointments = await _context.Appointments
            .Where(a => a.SeriesId == cmd.SeriesId
                        && a.Date >= today
                        && a.Status == AppointmentStatus.Scheduled)
            .ToListAsync(ct);

        if (futureAppointments.Count == 0)
            return Result<int>.NotFound("No future scheduled appointments found for this series");

        foreach (var appointment in futureAppointments)
        {
            appointment.Cancel("Series cancelled");
        }

        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(futureAppointments.Count);
    }
}
