using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Services;

internal class WorkingHoursReader : IWorkingHoursReader
{
    private readonly PreferencesDbContext _context;
    private readonly IClinicContext _clinicContext;

    public WorkingHoursReader(PreferencesDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<IReadOnlyList<WorkingHoursDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var hours = await _context.ClinicWorkingHours
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync(ct);

        if (hours.Count == 0)
        {
            var defaults = ClinicWorkingHours.CreateUaeDefaults(_clinicContext.ClinicId);
            var defaultDtos = defaults
                .Where(r => r.IsSuccess)
                .Select(r => r.Value.ToDto())
                .ToList();
            return Result<IReadOnlyList<WorkingHoursDto>>.Success(defaultDtos);
        }

        IReadOnlyList<WorkingHoursDto> dtos = hours.Select(h => h.ToDto()).ToList();
        return Result<IReadOnlyList<WorkingHoursDto>>.Success(dtos);
    }

    public async Task<Result<WorkingHoursDto>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken ct = default)
    {
        var entry = await _context.ClinicWorkingHours
            .FirstOrDefaultAsync(w => w.DayOfWeek == dayOfWeek, ct);

        if (entry is not null)
            return Result<WorkingHoursDto>.Success(entry.ToDto());

        // Return UAE default for the requested day
        var defaults = ClinicWorkingHours.CreateUaeDefaults(_clinicContext.ClinicId);
        var defaultResult = defaults.FirstOrDefault(r => r.IsSuccess && r.Value.DayOfWeek == dayOfWeek);

        if (defaultResult is null || !defaultResult.IsSuccess)
            return Result<WorkingHoursDto>.NotFound($"No working hours found for {dayOfWeek}");

        return Result<WorkingHoursDto>.Success(defaultResult.Value.ToDto());
    }
}
