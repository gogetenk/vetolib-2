using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;

internal record UpdateAppointmentStatusCommand(
    Guid AppointmentId,
    AppointmentStatus NewStatus,
    string? Reason) : IRequest<Result<AppointmentDto>>;
