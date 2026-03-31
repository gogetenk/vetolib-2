using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.ListWaitlistEntries;

internal class ListWaitlistEntriesHandler
    : IRequestHandler<ListWaitlistEntriesQuery, Result<List<WaitlistEntryDto>>>
{
    private readonly AgendaDbContext _context;

    public ListWaitlistEntriesHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<WaitlistEntryDto>>> Handle(
        ListWaitlistEntriesQuery query, CancellationToken ct)
    {
        var entries = await _context.WaitlistEntries
            .AsNoTracking()
            .OrderBy(e => e.CreatedAt)
            .Select(e => e.ToDto())
            .ToListAsync(ct);

        return Result<List<WaitlistEntryDto>>.Success(entries);
    }
}
