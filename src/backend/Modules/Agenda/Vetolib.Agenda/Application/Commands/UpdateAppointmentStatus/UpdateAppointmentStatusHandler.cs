using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;

internal class UpdateAppointmentStatusHandler
    : IRequestHandler<UpdateAppointmentStatusCommand, Result<AppointmentDto>>
{
    private readonly AgendaDbContext _context;

    public UpdateAppointmentStatusHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(
        UpdateAppointmentStatusCommand cmd, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<AppointmentDto>.NotFound($"Appointment {cmd.AppointmentId} not found");

        var transitionResult = cmd.NewStatus switch
        {
            AppointmentStatus.CheckedIn  => appointment.CheckIn(),
            AppointmentStatus.InProgress => appointment.StartConsultation(),
            AppointmentStatus.Completed  => appointment.Complete(),
            AppointmentStatus.Cancelled  => appointment.Cancel(cmd.Reason),
            AppointmentStatus.NoShow     => appointment.MarkNoShow(),
            _ => Result.Error($"UNSUPPORTED_TRANSITION:Transition to {cmd.NewStatus} is not supported")
        };

        if (!transitionResult.IsSuccess)
            return Result<AppointmentDto>.Error(string.Join("; ", transitionResult.Errors));

        await _context.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
