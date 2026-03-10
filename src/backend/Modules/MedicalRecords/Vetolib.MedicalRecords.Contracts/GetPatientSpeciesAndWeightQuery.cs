using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record GetPatientSpeciesAndWeightQuery(Guid PatientId) : IRequest<Result<PatientSpeciesWeightDto>>;
