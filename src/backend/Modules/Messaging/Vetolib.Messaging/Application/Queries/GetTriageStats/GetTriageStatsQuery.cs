using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetTriageStats;

internal record GetTriageStatsQuery : IRequest<Result<TriageStatsDto>>;
