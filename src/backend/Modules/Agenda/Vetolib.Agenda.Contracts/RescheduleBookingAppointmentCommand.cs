using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Reschedule a booking appointment via the owner portal (magic link authenticated).
/// Cancels the original and creates a new appointment with BookingSource=OwnerPortal,
/// RescheduleCount+1, and OriginalAppointmentId set.
/// </summary>
public record RescheduleBookingAppointmentCommand(
    Guid AppointmentId,
    Guid OwnerId,
    Guid ClinicId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    int NewDurationMinutes
) : IRequest<Result<AppointmentDto>>;
