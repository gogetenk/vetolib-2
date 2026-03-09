using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.EditAppointment;

internal record EditAppointmentCommand(
    Guid AppointmentId,
    DateOnly? Date,
    TimeOnly? StartTime,
    int? DurationMinutes,
    Guid? VeterinarianId,
    string? VeterinarianName,
    string? Reason,
    string? Notes) : IRequest<Result<AppointmentDto>>;
