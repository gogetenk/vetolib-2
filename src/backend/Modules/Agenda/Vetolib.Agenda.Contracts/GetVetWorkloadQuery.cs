using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Contracts;

/// <summary>
/// Returns the number of appointments per veterinarian for the current week (Monday-Sunday).
/// Dispatched from Vetolib.Api for the dashboard vet-workload endpoint.
/// </summary>
public record GetVetWorkloadQuery : IRequest<Result<IReadOnlyList<VetWorkloadDto>>>;

public record VetWorkloadDto(
    Guid VeterinarianId,
    string VeterinarianName,
    int AppointmentCount
);
