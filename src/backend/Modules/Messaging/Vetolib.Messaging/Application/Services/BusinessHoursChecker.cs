using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Services;

internal class BusinessHoursChecker : IBusinessHoursChecker
{
    private static readonly TimeZoneInfo DubaiTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Dubai");

    private readonly MessagingDbContext _context;

    public BusinessHoursChecker(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsWithinBusinessHoursAsync(Guid clinicId, DateTime utcNow, CancellationToken ct = default)
    {
        var dubaiTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, DubaiTimeZone);
        var dayOfWeek = (int)dubaiTime.DayOfWeek;
        var timeOfDay = TimeOnly.FromDateTime(dubaiTime);

        // Multi-tenant filter is applied by the DbContext automatically,
        // but here clinicId is passed explicitly for cases where the caller
        // is not operating under the current tenant context (e.g., background jobs).
        // We use IgnoreQueryFilters here intentionally for this service-layer lookup
        // where the clinicId is provided as an explicit parameter.
        var hoursForDay = await _context.MessagingHours
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.ClinicId == clinicId && h.DayOfWeek == dayOfWeek, ct);

        if (hoursForDay is null)
        {
            // No hours configured: use UAE default (Sunday-Thursday 08:00-20:00)
            return IsWithinUaeDefault(dayOfWeek, timeOfDay);
        }

        return hoursForDay.IsOpenAt(timeOfDay);
    }

    private static bool IsWithinUaeDefault(int dayOfWeek, TimeOnly time)
    {
        // UAE default: Sunday(0)-Thursday(4) 08:00-20:00, Friday(5) 08:00-12:00, Saturday(6) closed
        return dayOfWeek switch
        {
            0 or 1 or 2 or 3 or 4 => time >= new TimeOnly(8, 0) && time < new TimeOnly(20, 0),
            5 => time >= new TimeOnly(8, 0) && time < new TimeOnly(12, 0),
            _ => false
        };
    }
}
