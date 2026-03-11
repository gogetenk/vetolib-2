using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CancelBookingAppointment;

internal class CancelBookingAppointmentHandler : IRequestHandler<CancelBookingAppointmentCommand, Result>
{
    // Default minimum hours before appointment that owner can cancel (without clinic-specific preferences)
    private const int DefaultMinCancelHours = 24;

    private readonly AgendaDbContext _context;

    public CancelBookingAppointmentHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelBookingAppointmentCommand cmd, CancellationToken ct)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result.NotFound($"Appointment '{cmd.AppointmentId}' not found.");

        // Verify ownership
        if (appointment.OwnerId != cmd.OwnerId)
            return Result.Forbidden();

        // Verify status allows cancellation
        if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.CheckedIn)
            return Result.Error(
                $"INVALID_STATUS:Cannot cancel an appointment in status '{appointment.Status}'. Only Scheduled or CheckedIn appointments can be cancelled.");

        // Verify cancellation window (BookingMinCancelHours, default 24h)
        var appointmentDateTime = appointment.Date.ToDateTime(appointment.StartTime);
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;

        if (hoursUntilAppointment < DefaultMinCancelHours)
            return Result.Error(
                $"CANCEL_WINDOW_EXPIRED:Appointments can only be cancelled at least {DefaultMinCancelHours} hours in advance. This appointment starts in {Math.Floor(hoursUntilAppointment)} hours.");

        var cancelResult = appointment.Cancel(cmd.Reason ?? "Cancelled by owner via portal");
        if (!cancelResult.IsSuccess)
            return cancelResult;

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
