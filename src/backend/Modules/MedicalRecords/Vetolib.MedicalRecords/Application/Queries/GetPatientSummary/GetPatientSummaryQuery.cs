using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientSummary;

internal record GetPatientSummaryQuery(Guid PatientId) : IRequest<Result<PatientSummaryDto>>;
