using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record GetDrugAlternativesQuery(Guid DrugId, Guid? PatientId = null) : IRequest<Result<List<AlternativeDrugDto>>>;
