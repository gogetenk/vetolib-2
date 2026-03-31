using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.MarkWaitingRoom;

internal record MarkWaitingRoomCommand(Guid AppointmentId) : IRequest<Result<AppointmentDto>>;
