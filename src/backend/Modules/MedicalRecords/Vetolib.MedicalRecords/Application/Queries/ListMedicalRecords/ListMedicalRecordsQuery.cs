using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;

internal record ListMedicalRecordsQuery(Guid PatientId) : IRequest<Result<List<MedicalRecordDto>>>;
