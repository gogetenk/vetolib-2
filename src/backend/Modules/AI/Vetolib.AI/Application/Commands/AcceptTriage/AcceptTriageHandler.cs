using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Commands.AcceptTriage;

internal class AcceptTriageHandler : IRequestHandler<AcceptTriageCommand, Result>
{
    private readonly AIDbContext _context;

    public AcceptTriageHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AcceptTriageCommand cmd, CancellationToken ct)
    {
        var triageResult = await _context.TriageResults
            .FirstOrDefaultAsync(t => t.Id == cmd.TriageId, ct);

        if (triageResult is null)
            return Result.NotFound($"Triage result {cmd.TriageId} not found.");

        triageResult.Accept();
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
