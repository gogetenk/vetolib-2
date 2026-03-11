using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;

internal record ListMedicalRecordsQuery(Guid PatientId, int Page = 1, int PageSize = 20)
    : IRequest<Result<MedicalRecordPagedResultDto>>;
