using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateAppointment;

internal record CreateAppointmentCommand(
    Guid ClinicId,
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason) : IRequest<Result<AppointmentDto>>;
