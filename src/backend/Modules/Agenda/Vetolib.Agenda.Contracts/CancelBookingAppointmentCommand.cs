using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Cancel a booking appointment via the owner portal (magic link authenticated).
/// OwnerId is extracted from the portal token and used to verify ownership.
/// </summary>
public record CancelBookingAppointmentCommand(
    Guid AppointmentId,
    Guid OwnerId,
    Guid ClinicId,
    string? Reason = null
) : IRequest<Result>;
