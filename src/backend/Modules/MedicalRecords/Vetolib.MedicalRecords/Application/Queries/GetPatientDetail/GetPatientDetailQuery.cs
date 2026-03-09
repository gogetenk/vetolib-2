using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientDetail;

internal record GetPatientDetailQuery(Guid PatientId) : IRequest<Result<PatientDetailDto>>;
