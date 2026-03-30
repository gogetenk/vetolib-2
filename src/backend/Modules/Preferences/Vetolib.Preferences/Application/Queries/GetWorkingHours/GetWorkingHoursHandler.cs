using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Preferences.Application.Domain;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Queries.GetWorkingHours;

internal class GetWorkingHoursHandler : IRequestHandler<GetWorkingHoursQuery, Result<IReadOnlyList<WorkingHoursDto>>>
{
    private readonly PreferencesDbContext _context;
    private readonly IClinicContext _clinicContext;

    public GetWorkingHoursHandler(PreferencesDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<IReadOnlyList<WorkingHoursDto>>> Handle(
        GetWorkingHoursQuery query, CancellationToken ct)
    {
        var hours = await _context.ClinicWorkingHours
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync(ct);

        // If no working hours configured yet, return UAE defaults (not persisted)
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
}
