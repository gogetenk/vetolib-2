using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

/// <summary>
/// Public query dispatched from Vetolib.Api for the dashboard.
/// Handler lives in Vetolib.MedicalRecords (internal).
/// </summary>
public record GetPatientCountQuery : IRequest<Result<int>>;
