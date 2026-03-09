using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Returns appointment analytics for the last 30 days:
/// - noShowRate: percentage of CANCELLED or NOSHOW (non-completed) appointments
/// - appointmentsByStatus: distribution per status
/// Dispatched from Vetolib.Api for the analytics dashboard endpoint.
/// </summary>
public record GetAppointmentsAnalyticsQuery : IRequest<Result<AppointmentsAnalyticsDto>>;

public record AppointmentsAnalyticsDto(
    decimal NoShowRate,
    IReadOnlyList<AppointmentStatusCountDto> AppointmentsByStatus
);

public record AppointmentStatusCountDto(
    string Status,
    int Count
);
