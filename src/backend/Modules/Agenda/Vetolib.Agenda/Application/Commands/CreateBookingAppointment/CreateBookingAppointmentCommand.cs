using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateBookingAppointment;

/// <summary>
/// Creates an appointment via the owner booking portal (BookingSource.OwnerPortal).
/// OwnerId and ClinicId are extracted from the validated MagicLink token by the endpoint filter.
/// </summary>
internal record CreateBookingAppointmentCommand(
    Guid ClinicId,
    Guid OwnerId,
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason) : IRequest<Result<AppointmentDto>>;
