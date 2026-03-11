using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetOwnerAppointmentById;

/// <summary>
/// Returns a single appointment for an owner, verifying ownership.
/// Used by the MagicLink-authenticated booking portal.
/// </summary>
internal record GetOwnerAppointmentByIdQuery(Guid AppointmentId, Guid OwnerId, Guid ClinicId)
    : IRequest<Result<AppointmentDto>>;
