using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetWaitingRoom;

internal record GetWaitingRoomQuery() : IRequest<Result<List<WaitingRoomAppointmentDto>>>;
