using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientById;

internal record GetPatientByIdQuery(Guid PatientId) : IRequest<Result<PatientDto>>;
