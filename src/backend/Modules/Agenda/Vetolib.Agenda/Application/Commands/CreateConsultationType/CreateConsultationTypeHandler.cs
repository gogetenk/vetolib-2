using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.CreateConsultationType;

internal class CreateConsultationTypeHandler : IRequestHandler<CreateConsultationTypeCommand, Result<ConsultationTypeDto>>
{
    private readonly AgendaDbContext _context;

    public CreateConsultationTypeHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConsultationTypeDto>> Handle(CreateConsultationTypeCommand cmd, CancellationToken ct)
    {
        var duplicateExists = await _context.ConsultationTypes
            .AnyAsync(c => c.Name == cmd.Name && c.IsActive, ct);

        if (duplicateExists)
            return Result<ConsultationTypeDto>.Conflict($"An active consultation type named '{cmd.Name}' already exists.");

        var createResult = ConsultationType.Create(
            cmd.ClinicId,
            cmd.Name,
            cmd.DurationMinutes,
            cmd.SortOrder,
            cmd.RequiresVetSelection);

        if (!createResult.IsSuccess)
            return createResult.Map(_ => (ConsultationTypeDto)null!);

        _context.ConsultationTypes.Add(createResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<ConsultationTypeDto>.Success(createResult.Value.ToDto());
    }
}
