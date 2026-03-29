using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetWeightHistory;

internal record GetWeightHistoryQuery(Guid PatientId, int Page = 1, int PageSize = 20)
    : IRequest<Result<WeightHistoryResultDto>>;
