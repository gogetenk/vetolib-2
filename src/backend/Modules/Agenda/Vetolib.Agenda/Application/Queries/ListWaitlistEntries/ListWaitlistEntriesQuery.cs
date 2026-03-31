using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListWaitlistEntries;

internal record ListWaitlistEntriesQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<WaitlistPagedResultDto>>;
