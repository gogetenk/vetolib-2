using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Application.Commands.DeleteStaffSchedule;

internal record DeleteStaffScheduleCommand(Guid Id) : IRequest<Result>;
