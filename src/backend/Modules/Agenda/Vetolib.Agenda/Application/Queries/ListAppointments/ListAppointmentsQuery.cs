using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListAppointments;

internal record ListAppointmentsQuery(DateOnly Date) : IRequest<Result<IReadOnlyList<AppointmentDto>>>;
