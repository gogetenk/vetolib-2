using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupRevenueComparison;

internal record GetGroupRevenueComparisonQuery(Guid GroupId, Guid RequestingUserId)
    : IRequest<Result<ClinicGroupRevenueComparisonDto>>;
