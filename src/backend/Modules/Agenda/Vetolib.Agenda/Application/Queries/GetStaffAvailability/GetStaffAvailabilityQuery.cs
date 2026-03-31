using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetStaffAvailability;

internal record GetStaffAvailabilityQuery(DateOnly Date) : IRequest<Result<List<StaffScheduleDto>>>;
