using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.UpdateConsultationType;

internal class UpdateConsultationTypeHandler : IRequestHandler<UpdateConsultationTypeCommand, Result<ConsultationTypeDto>>
{
    private readonly AgendaDbContext _context;

    public UpdateConsultationTypeHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConsultationTypeDto>> Handle(UpdateConsultationTypeCommand cmd, CancellationToken ct)
    {
        var consultationType = await _context.ConsultationTypes
            .FirstOrDefaultAsync(c => c.Id == cmd.Id, ct);

        if (consultationType is null)
            return Result<ConsultationTypeDto>.NotFound($"ConsultationType '{cmd.Id}' not found.");

        var updateResult = consultationType.Update(cmd.Name, cmd.DurationMinutes, cmd.SortOrder, cmd.RequiresVetSelection);
        if (!updateResult.IsSuccess)
            return updateResult.Map(_ => (ConsultationTypeDto)null!);

        await _context.SaveChangesAsync(ct);

        return Result<ConsultationTypeDto>.Success(consultationType.ToDto());
    }
}
