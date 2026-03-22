using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Queries.GetTriageStats;

internal class GetTriageStatsHandler : IRequestHandler<GetTriageStatsQuery, Result<TriageStatsDto>>
{
    private readonly MessagingDbContext _context;
    private readonly MessagingOptions _options;

    public GetTriageStatsHandler(MessagingDbContext context, IOptions<MessagingOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Result<TriageStatsDto>> Handle(GetTriageStatsQuery query, CancellationToken ct)
    {
        var windowStart = DateTime.UtcNow.AddDays(-_options.TriageStatsWindowDays);

        var recentConversations = _context.Conversations
            .AsNoTracking()
            .Where(c => c.CreatedAt >= windowStart);

        // Average first response time — computed via DB aggregation, no Include(Messages)
        var responseTimes = await _context.Messages
            .AsNoTracking()
            .Where(m => recentConversations.Select(c => c.Id).Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new
            {
                OwnerFirstSentAt = g
                    .Where(m => m.Sender == MessageSender.Owner)
                    .OrderBy(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault(),
                StaffFirstSentAt = g
                    .Where(m => m.Sender == MessageSender.Vet || m.Sender == MessageSender.Staff)
                    .OrderBy(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault()
            })
            .Where(x => x.OwnerFirstSentAt != null && x.StaffFirstSentAt != null && x.StaffFirstSentAt > x.OwnerFirstSentAt)
            .Select(x => (x.StaffFirstSentAt!.Value - x.OwnerFirstSentAt!.Value).TotalMinutes)
            .ToListAsync(ct);

        var avgResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0.0;

        // Messages by category — aggregated in DB
        var messagesByCategory = await recentConversations
            .GroupBy(c => c.Category)
            .Select(g => new CategoryCountDto(g.Key, g.Count()))
            .ToListAsync(ct);

        // AI triage accuracy
        var totalConversations = await recentConversations.CountAsync(ct);
        var triageUncertainCount = await recentConversations.CountAsync(c => c.IsTriageUncertain, ct);
        var accuracyPercent = totalConversations > 0
            ? (1.0 - (double)triageUncertainCount / totalConversations) * 100.0
            : 100.0;

        // Volume per day — aggregated in DB
        var volumePerDay = await recentConversations
            .GroupBy(c => DateOnly.FromDateTime(c.CreatedAt))
            .OrderBy(g => g.Key)
            .Select(g => new DailyVolumeDto(g.Key, g.Count()))
            .ToListAsync(ct);

        // Conversion rate
        var appointmentRequests = await recentConversations
            .CountAsync(c => c.Category == MessageCategory.AppointmentRequest, ct);
        var resolvedAppointmentRequests = await recentConversations
            .CountAsync(c =>
                c.Category == MessageCategory.AppointmentRequest &&
                (c.Status == ConversationStatus.Resolved || c.Status == ConversationStatus.Closed), ct);

        var conversionRate = appointmentRequests > 0
            ? (double)resolvedAppointmentRequests / appointmentRequests * 100.0
            : 0.0;

        var stats = new TriageStatsDto(
            AverageFirstResponseTime: avgResponseTime,
            MessagesByCategory: messagesByCategory,
            AiTriageAccuracy: accuracyPercent,
            VolumePerDay: volumePerDay,
            ConversionRateToAppointment: conversionRate);

        return Result<TriageStatsDto>.Success(stats);
    }
}
