using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Commands.OverrideTriage;

internal class OverrideTriageHandler : IRequestHandler<OverrideTriageCommand, Result>
{
    private readonly AIDbContext _context;

    public OverrideTriageHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(OverrideTriageCommand cmd, CancellationToken ct)
    {
        var triageResult = await _context.TriageResults
            .FirstOrDefaultAsync(t => t.Id == cmd.TriageId && t.ClinicId == cmd.ClinicId, ct);

        if (triageResult is null)
            return Result.NotFound($"Triage result {cmd.TriageId} not found.");

        triageResult.Override(cmd.NewSeverity);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
