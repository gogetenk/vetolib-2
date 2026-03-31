using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.GetGroupClinicStats;

internal record GetGroupClinicStatsQuery(Guid GroupId, Guid RequestingUserId)
    : IRequest<Result<IReadOnlyList<ClinicGroupClinicStatsDto>>>;
