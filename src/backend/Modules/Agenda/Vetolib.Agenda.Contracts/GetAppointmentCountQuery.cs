using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Returns the total number of appointments for the current clinic.
/// Handler lives in Vetolib.Agenda (internal).
/// Used by Auth module for onboarding auto-completion.
/// </summary>
public record GetAppointmentCountQuery : IRequest<Result<int>>;
