using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListStaffSchedules;

internal record ListStaffSchedulesQuery(DateOnly From, DateOnly To) : IRequest<Result<List<StaffScheduleDto>>>;
