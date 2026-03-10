using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Contracts.Events;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Services;

/// <summary>
/// Background service that scans every minute for MedicalUrgency conversations
/// that have been open for more than 10 minutes without a vet viewing them,
/// and publishes an EmergencyEscalationEvent once per conversation.
/// </summary>
internal class EmergencyEscalationBackgroundService : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan EscalationThreshold = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmergencyEscalationBackgroundService> _logger;

    public EmergencyEscalationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EmergencyEscalationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmergencyEscalationBackgroundService started");

        using var timer = new PeriodicTimer(ScanInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ScanAndEscalateAsync(stoppingToken);
        }
    }

    private async Task ScanAndEscalateAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var threshold = DateTime.UtcNow - EscalationThreshold;

            // Find MedicalUrgency conversations that are Open, older than 10 minutes,
            // and have not yet been escalated.
            // IgnoreQueryFilters because this is a cross-clinic background scan.
            var conversationsToEscalate = await context.Conversations
                .IgnoreQueryFilters()
                .Include(c => c.Messages)
                .Where(c =>
                    c.Category == MessageCategory.MedicalUrgency &&
                    c.Status == ConversationStatus.Open &&
                    c.CreatedAt < threshold &&
                    c.EscalationSentAt == null)
                .ToListAsync(ct);

            foreach (var conversation in conversationsToEscalate)
            {
                var firstOwnerMessage = conversation.Messages
                    .Where(m => m.Sender == MessageSender.Owner)
                    .OrderBy(m => m.SentAt)
                    .FirstOrDefault();

                var preview = firstOwnerMessage is not null
                    ? (firstOwnerMessage.Body.Length > 100
                        ? firstOwnerMessage.Body[..100] + "..."
                        : firstOwnerMessage.Body)
                    : "(no message)";

                conversation.MarkEscalationSent();

                await publishEndpoint.Publish(new EmergencyEscalationEvent(
                    conversation.Id,
                    conversation.ClinicId,
                    preview), ct);

                _logger.LogWarning(
                    "Emergency escalation published for conversation {ConversationId} in clinic {ClinicId}",
                    conversation.Id,
                    conversation.ClinicId);
            }

            if (conversationsToEscalate.Count > 0)
                await context.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during emergency escalation scan");
        }
    }
}
