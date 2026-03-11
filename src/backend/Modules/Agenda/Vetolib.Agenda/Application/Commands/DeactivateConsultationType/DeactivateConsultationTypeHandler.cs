using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.DeactivateConsultationType;

internal class DeactivateConsultationTypeHandler : IRequestHandler<DeactivateConsultationTypeCommand, Result>
{
    private readonly AgendaDbContext _context;

    public DeactivateConsultationTypeHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateConsultationTypeCommand cmd, CancellationToken ct)
    {
        var consultationType = await _context.ConsultationTypes
            .FirstOrDefaultAsync(c => c.Id == cmd.Id, ct);

        if (consultationType is null)
            return Result.NotFound($"ConsultationType '{cmd.Id}' not found.");

        var deactivateResult = consultationType.Deactivate();
        if (!deactivateResult.IsSuccess)
            return deactivateResult;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
