using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.GetOwnerAppointmentById;

internal class GetOwnerAppointmentByIdHandler
    : IRequestHandler<GetOwnerAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;

    public GetOwnerAppointmentByIdHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(GetOwnerAppointmentByIdQuery query, CancellationToken ct)
    {
        // IgnoreQueryFilters() because portal requests have no IClinicContext populated.
        // We explicitly verify both ClinicId and OwnerId for data isolation.
        var appointment = await _context.Appointments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == query.AppointmentId
                     && a.ClinicId == query.ClinicId,
                ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment {query.AppointmentId} not found.");

        // Verify the appointment belongs to this owner
        if (appointment.OwnerId != query.OwnerId)
            return Result<AppointmentDto>.Forbidden();

        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
