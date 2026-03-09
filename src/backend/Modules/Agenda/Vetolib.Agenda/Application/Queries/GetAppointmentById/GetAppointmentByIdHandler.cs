using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetAppointmentById;

internal class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;

    public GetAppointmentByIdHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery query, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.AppointmentId, ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment {query.AppointmentId} not found.");

        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
