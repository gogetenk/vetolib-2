using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;

namespace Vetolib.Preferences.Application.Commands.UpsertWorkingHours;

internal class UpsertWorkingHoursHandler : IRequestHandler<UpsertWorkingHoursCommand, Result<IReadOnlyList<WorkingHoursDto>>>
{
    private readonly PreferencesDbContext _context;

    public UpsertWorkingHoursHandler(PreferencesDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<WorkingHoursDto>>> Handle(
        UpsertWorkingHoursCommand cmd, CancellationToken ct)
    {
        var existing = await _context.ClinicWorkingHours
            .ToListAsync(ct);

        var results = new List<WorkingHoursDto>();

        foreach (var day in cmd.Days)
        {
            var entry = existing.FirstOrDefault(e => e.DayOfWeek == day.DayOfWeek);

            if (entry is not null)
            {
                var updateResult = entry.Update(
                    day.IsOpen, day.OpenTime, day.CloseTime,
                    day.BreakStartTime, day.BreakEndTime);

                if (!updateResult.IsSuccess)
                    return Result<IReadOnlyList<WorkingHoursDto>>.Invalid(updateResult.ValidationErrors.ToList());

                results.Add(entry.ToDto());
            }
            else
            {
                var createResult = ClinicWorkingHours.Create(
                    cmd.ClinicId, day.DayOfWeek, day.IsOpen,
                    day.OpenTime, day.CloseTime,
                    day.BreakStartTime, day.BreakEndTime);

                if (!createResult.IsSuccess)
                    return Result<IReadOnlyList<WorkingHoursDto>>.Invalid(createResult.ValidationErrors.ToList());

                _context.ClinicWorkingHours.Add(createResult.Value);
                results.Add(createResult.Value.ToDto());
            }
        }

        await _context.SaveChangesAsync(ct);

        return Result<IReadOnlyList<WorkingHoursDto>>.Success(results);
    }
}
