using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Commands.NotifyWaitlist;

internal class SlotAvailableEventHandler : INotificationHandler<SlotAvailableEvent>
{
    private readonly AgendaDbContext _context;
    private readonly ILogger<SlotAvailableEventHandler> _logger;

    public SlotAvailableEventHandler(AgendaDbContext context, ILogger<SlotAvailableEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(SlotAvailableEvent notification, CancellationToken ct)
    {
        var timeSlot = GetTimeSlot(notification.StartTime);

        // Find matching pending waitlist entries for this date
        var matchingEntries = await _context.WaitlistEntries
            .Where(e => e.Status == WaitlistEntryStatus.Pending)
            .Where(e => e.PreferredDate == notification.Date)
            .Where(e => e.PreferredTimeSlot == PreferredTimeSlot.Any || e.PreferredTimeSlot == timeSlot)
            .Where(e => e.VetPreference == null || e.VetPreference == notification.VeterinarianId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);

        foreach (var entry in matchingEntries)
        {
            var notifyResult = entry.Notify();
            if (notifyResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Notified waitlist entry {EntryId} for patient {PatientId} about available slot on {Date} at {Time}",
                    entry.Id, entry.PatientId, notification.Date, notification.StartTime);
            }
        }

        if (matchingEntries.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
        }
    }

    private static PreferredTimeSlot GetTimeSlot(TimeOnly time)
    {
        return time.Hour switch
        {
            < 12 => PreferredTimeSlot.Morning,
            < 17 => PreferredTimeSlot.Afternoon,
            _ => PreferredTimeSlot.Evening
        };
    }
}
