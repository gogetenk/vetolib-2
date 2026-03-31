using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetMySchedule;

internal record GetMyScheduleQuery(Guid UserId, DateOnly From, DateOnly To) : IRequest<Result<List<StaffScheduleDto>>>;
