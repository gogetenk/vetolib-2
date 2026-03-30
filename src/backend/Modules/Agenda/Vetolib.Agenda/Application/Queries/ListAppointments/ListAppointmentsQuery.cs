using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListAppointments;

internal record ListAppointmentsQuery(DateOnly Date, int PageNumber = 1, int PageSize = 50)
    : IRequest<Result<AppointmentPagedResultDto>>;
