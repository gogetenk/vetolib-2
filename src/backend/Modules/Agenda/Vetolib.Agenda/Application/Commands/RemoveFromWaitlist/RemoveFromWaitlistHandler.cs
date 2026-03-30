using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.RemoveFromWaitlist;

internal class RemoveFromWaitlistHandler
    : IRequestHandler<RemoveFromWaitlistCommand, Result>
{
    private readonly AgendaDbContext _context;

    public RemoveFromWaitlistHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveFromWaitlistCommand cmd, CancellationToken ct)
    {
        var entry = await _context.WaitlistEntries
            .FirstOrDefaultAsync(e => e.Id == cmd.EntryId, ct);

        if (entry is null)
            return Result.NotFound($"Waitlist entry {cmd.EntryId} not found");

        _context.WaitlistEntries.Remove(entry);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
