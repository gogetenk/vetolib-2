using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListWaitlistEntries;

internal class ListWaitlistEntriesHandler
    : IRequestHandler<ListWaitlistEntriesQuery, Result<WaitlistPagedResultDto>>
{
    private readonly AgendaDbContext _context;

    public ListWaitlistEntriesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WaitlistPagedResultDto>> Handle(
        ListWaitlistEntriesQuery query, CancellationToken ct)
    {
        var page = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 200);

        var baseQuery = _context.WaitlistEntries
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(ct);

        var entries = await baseQuery
            .OrderBy(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => e.ToDto())
            .ToListAsync(ct);

        return Result<WaitlistPagedResultDto>.Success(
            new WaitlistPagedResultDto(entries, totalCount, page, pageSize));
    }
}
