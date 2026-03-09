using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Services;

/// <summary>
/// Estimates appointment duration via sliding average of the last 20 completed appointments
/// for the same vet + consultation type. Falls back to the configured default if fewer than 5.
/// </summary>
internal class DurationEstimator
{
    private readonly AgendaDbContext _context;
    private readonly AgendaOptions _options;

    public DurationEstimator(AgendaDbContext context, IOptions<AgendaOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<int>> EstimateAsync(
        Guid veterinarianId,
        string consultationType,
        CancellationToken ct = default)
    {
        // Use IgnoreQueryFilters only for historical cross-tenant aggregate data would be wrong —
        // but here we stay inside the tenant. The MultiTenant filter on Appointments is already
        // scoped to the current clinic so we do NOT need IgnoreQueryFilters.
        var history = await _context.Appointments
            .Where(a =>
                a.VeterinarianId == veterinarianId &&
                a.Reason != null &&
                a.Reason.ToLower().StartsWith(consultationType.ToLower()) &&
                a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.Date)
            .Take(20)
            .Select(a => a.DurationMinutes)
            .ToListAsync(ct);

        if (history.Count < 5)
        {
            var defaultDuration = GetDefaultDuration(consultationType);
            return Result<int>.Success(defaultDuration);
        }

        var average = (int)Math.Round(history.Average());
        return Result<int>.Success(average);
    }

    public int GetDefaultDuration(string consultationType)
    {
        var key = consultationType.ToLowerInvariant();
        if (_options.DefaultDurationByType.TryGetValue(key, out var duration))
            return duration;

        // Ultimate fallback
        return 30;
    }
}
