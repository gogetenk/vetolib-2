using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.GetSharedRecord;

internal record GetSharedRecordQuery(string Token) : IRequest<Result<PatientSummaryDto>>;
