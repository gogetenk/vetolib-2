using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupDashboardStats;

internal record GetGroupDashboardStatsQuery(Guid GroupId, Guid RequestingUserId)
    : IRequest<Result<ClinicGroupDashboardStatsDto>>;
