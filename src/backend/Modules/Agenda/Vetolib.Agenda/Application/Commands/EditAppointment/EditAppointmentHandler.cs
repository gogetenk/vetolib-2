using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.EditAppointment;

internal class EditAppointmentHandler : IRequestHandler<EditAppointmentCommand, Result<AppointmentDto>>
{
    private const int MaxRetries = 3;
    private readonly AgendaDbContext _context;
    private readonly IOutputCacheStore? _cache;

    public EditAppointmentHandler(AgendaDbContext context, IOutputCacheStore? cache = null)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<AppointmentDto>> Handle(EditAppointmentCommand cmd, CancellationToken ct)
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

            if (appointment is null)
                return Result<AppointmentDto>.NotFound($"Rendez-vous {cmd.AppointmentId} introuvable");

            // Apply domain logic (guards status, etc.)
            var rescheduleResult = appointment.Reschedule(
                cmd.Date,
                cmd.StartTime,
                cmd.DurationMinutes,
                cmd.VeterinarianId,
                cmd.VeterinarianName,
                cmd.Reason,
                cmd.Notes);

            if (!rescheduleResult.IsSuccess)
                return Result<AppointmentDto>.Error(string.Join("; ", rescheduleResult.Errors));

            // Check for conflicts on the new slot (excluding this appointment itself)
            var newDate = cmd.Date ?? appointment.Date;
            var newVetId = (cmd.VeterinarianId.HasValue && cmd.VeterinarianId.Value != Guid.Empty)
                ? cmd.VeterinarianId.Value
                : appointment.VeterinarianId;

            var conflictingAppointments = await _context.Appointments
                .Where(a => a.VeterinarianId == newVetId
                            && a.Date == newDate
                            && a.Id != cmd.AppointmentId
                            && a.Status != AppointmentStatus.Cancelled)
                .ToListAsync(ct);

            var conflict = conflictingAppointments
                .FirstOrDefault(a => a.OverlapsWith(appointment.StartTime, appointment.EndTime));

            if (conflict is not null)
                return Result<AppointmentDto>.Error(
                    "APPOINTMENT_CONFLICT:Ce creneau est deja pris pour ce veterinaire.");

            try
            {
                await _context.SaveChangesAsync(ct);
                if (_cache is not null) await _cache.EvictByTagAsync("dashboard", ct);
                return Result<AppointmentDto>.Success(appointment.ToDto());
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Unique constraint IX_appointments_vet_date_starttime_unique was violated by a
                // concurrent edit — clear tracker and retry
                _context.ChangeTracker.Clear();
            }
        }

        return Result<AppointmentDto>.Error(
            "APPOINTMENT_CONFLICT:Ce creneau vient d'etre pris simultanement. Veuillez choisir un autre creneau.");
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("23505") == true
            || ex.InnerException?.Message.Contains("IX_appointments_vet_date_starttime_unique") == true
            || ex.InnerException?.Message.Contains("unique") == true;
    }
}
