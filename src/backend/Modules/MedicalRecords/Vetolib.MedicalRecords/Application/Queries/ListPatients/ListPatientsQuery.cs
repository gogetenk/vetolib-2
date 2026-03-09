using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListPatients;

internal record ListPatientsQuery(
    string? Name = null,
    Species? Species = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PatientPagedResult>>;

internal record PatientPagedResult(
    List<PatientDto> Items,
    int Total,
    int Page,
    int PageSize);
