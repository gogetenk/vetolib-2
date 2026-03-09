using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Returns patient counts grouped by species.
/// Dispatched from Vetolib.Api for the analytics dashboard endpoint.
/// </summary>
public record GetPatientsBySpeciesQuery : IRequest<Result<IReadOnlyList<PatientsBySpeciesDto>>>;

public record PatientsBySpeciesDto(
    string Species,
    int Count
);
