using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Returns the number of appointments per consultation reason for the current month.
/// Dispatched from Vetolib.Api for the dashboard consultation-breakdown endpoint.
/// </summary>
public record GetConsultationBreakdownQuery : IRequest<Result<IReadOnlyList<ConsultationBreakdownDto>>>;

public record ConsultationBreakdownDto(
    string ConsultationType,
    int Count
);
