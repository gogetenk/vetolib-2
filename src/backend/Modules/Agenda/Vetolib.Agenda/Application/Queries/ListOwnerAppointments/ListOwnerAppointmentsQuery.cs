using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListOwnerAppointments;

/// <summary>
/// Returns all appointments for a given owner, sorted by date descending.
/// Used by the MagicLink-authenticated booking portal.
/// OwnerId and ClinicId are provided by the portal filter (not from HTTP body).
/// </summary>
internal record ListOwnerAppointmentsQuery(Guid OwnerId, Guid ClinicId)
    : IRequest<Result<List<AppointmentDto>>>;
