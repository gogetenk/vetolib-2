using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetAppointmentById;

internal record GetAppointmentByIdQuery(Guid AppointmentId) : IRequest<Result<AppointmentDto>>;
