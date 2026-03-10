using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetTriageStats;

internal class GetTriageStatsHandler : IRequestHandler<GetTriageStatsQuery, Result<TriageStatsDto>>
{
    private readonly MessagingDbContext _context;

    public GetTriageStatsHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TriageStatsDto>> Handle(GetTriageStatsQuery query, CancellationToken ct)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var conversations = await _context.Conversations
            .AsNoTracking()
            .Include(c => c.Messages)
            .Where(c => c.CreatedAt >= thirtyDaysAgo)
            .ToListAsync(ct);

        // Average first response time (in minutes): time from first owner message to first staff reply
        var responseTimes = new List<double>();
        foreach (var conversation in conversations)
        {
            var messages = conversation.Messages.OrderBy(m => m.SentAt).ToList();
            var ownerFirst = messages.FirstOrDefault(m => m.Sender == MessageSender.Owner);
            var staffFirst = messages.FirstOrDefault(m => m.Sender == MessageSender.Vet || m.Sender == MessageSender.Staff);

            if (ownerFirst is not null && staffFirst is not null && staffFirst.SentAt > ownerFirst.SentAt)
            {
                responseTimes.Add((staffFirst.SentAt - ownerFirst.SentAt).TotalMinutes);
            }
        }

        var avgResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0.0;

        // Messages by category
        var messagesByCategory = conversations
            .GroupBy(c => c.Category)
            .Select(g => new CategoryCountDto(g.Key, g.Count()))
            .ToList();

        // AI triage accuracy: percentage of conversations NOT re-categorized by staff
        // IsTriageUncertain == true means AI was uncertain; if re-categorized, means staff overrode
        var totalConversations = conversations.Count;
        var triageUncertainCount = conversations.Count(c => c.IsTriageUncertain);
        var accuracyPercent = totalConversations > 0
            ? (1.0 - (double)triageUncertainCount / totalConversations) * 100.0
            : 100.0;

        // Volume per day (last 30 days)
        var volumePerDay = conversations
            .GroupBy(c => DateOnly.FromDateTime(c.CreatedAt))
            .OrderBy(g => g.Key)
            .Select(g => new DailyVolumeDto(g.Key, g.Count()))
            .ToList();

        // Conversion rate: conversations with category AppointmentRequest that led to an appointment
        // For now we track conversations that were closed/resolved as a proxy
        var appointmentRequests = conversations.Count(c => c.Category == MessageCategory.AppointmentRequest);
        var resolvedAppointmentRequests = conversations.Count(c =>
            c.Category == MessageCategory.AppointmentRequest &&
            (c.Status == ConversationStatus.Resolved || c.Status == ConversationStatus.Closed));

        var conversionRate = appointmentRequests > 0
            ? (double)resolvedAppointmentRequests / appointmentRequests * 100.0
            : 0.0;

        var stats = new TriageStatsDto(
            avgResponseTime,
            messagesByCategory,
            accuracyPercent,
            volumePerDay,
            conversionRate);

        return Result<TriageStatsDto>.Success(stats);
    }
}
