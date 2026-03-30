using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.AddToWaitlist;

internal class AddToWaitlistHandler
    : IRequestHandler<AddToWaitlistCommand, Result<WaitlistEntryDto>>
{
    private readonly AgendaDbContext _context;

    public AddToWaitlistHandler(AgendaDbContext context)
    {
        _context = context;
    }

    public async Task<Result<WaitlistEntryDto>> Handle(
        AddToWaitlistCommand cmd, CancellationToken ct)
    {
        var createResult = WaitlistEntry.Create(
            cmd.ClinicId,
            cmd.PatientId,
            cmd.OwnerName,
            cmd.OwnerPhone,
            cmd.OwnerEmail,
            cmd.PreferredDate,
            cmd.PreferredTimeSlot,
            cmd.VetPreference,
            cmd.Reason);

        if (!createResult.IsSuccess)
            return Result<WaitlistEntryDto>.Invalid(createResult.ValidationErrors.ToList());

        _context.WaitlistEntries.Add(createResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<WaitlistEntryDto>.Success(createResult.Value.ToDto());
    }
}
