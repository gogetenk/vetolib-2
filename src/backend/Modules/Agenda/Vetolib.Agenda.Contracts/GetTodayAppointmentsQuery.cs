using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Public query dispatched from Vetolib.Api for the dashboard.
/// Handler lives in Vetolib.Agenda (internal).
/// </summary>
public record GetTodayAppointmentsQuery : IRequest<Result<IReadOnlyList<AppointmentDto>>>;
