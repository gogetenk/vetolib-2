using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.SearchClinics;

internal record SearchClinicsQuery(
    string? Name,
    string? City,
    string? Species,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<ClinicSearchPagedResultDto>>;
