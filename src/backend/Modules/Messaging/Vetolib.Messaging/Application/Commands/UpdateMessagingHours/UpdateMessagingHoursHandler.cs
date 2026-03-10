using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Commands.UpdateMessagingHours;

internal class UpdateMessagingHoursHandler : IRequestHandler<UpdateMessagingHoursCommand, Result<IReadOnlyList<MessagingHoursDto>>>
{
    private readonly MessagingDbContext _context;
    private readonly IClinicContext _clinicContext;

    public UpdateMessagingHoursHandler(MessagingDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<IReadOnlyList<MessagingHoursDto>>> Handle(UpdateMessagingHoursCommand command, CancellationToken ct)
    {
        var clinicId = _clinicContext.ClinicId;

        var existingHours = await _context.MessagingHours
            .ToListAsync(ct);

        var updatedDtos = new List<MessagingHoursDto>();

        foreach (var dayRequest in command.Days)
        {
            var existing = existingHours.FirstOrDefault(h => h.DayOfWeek == dayRequest.DayOfWeek);

            if (existing is not null)
            {
                var updateResult = existing.Update(dayRequest.OpenTime, dayRequest.CloseTime, dayRequest.IsClosed);
                if (!updateResult.IsSuccess)
                    return Result<IReadOnlyList<MessagingHoursDto>>.Invalid(updateResult.ValidationErrors);

                updatedDtos.Add(existing.ToDto());
            }
            else
            {
                var createResult = MessagingHours.Create(
                    clinicId,
                    dayRequest.DayOfWeek,
                    dayRequest.OpenTime,
                    dayRequest.CloseTime,
                    dayRequest.IsClosed);

                if (!createResult.IsSuccess)
                    return Result<IReadOnlyList<MessagingHoursDto>>.Invalid(createResult.ValidationErrors);

                _context.MessagingHours.Add(createResult.Value);
                updatedDtos.Add(createResult.Value.ToDto());
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result<IReadOnlyList<MessagingHoursDto>>.Success(updatedDtos);
    }
}
