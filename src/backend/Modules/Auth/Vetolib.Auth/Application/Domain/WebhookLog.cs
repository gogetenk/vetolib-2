using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class WebhookLog : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid WebhookRegistrationId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public WebhookLogStatus Status { get; private set; }

    private WebhookLog() { } // EF Core constructor

    public static WebhookLog Create(Guid clinicId, Guid webhookRegistrationId, string eventType, string payload)
    {
        return new WebhookLog
        {
            ClinicId = clinicId,
            WebhookRegistrationId = webhookRegistrationId,
            EventType = eventType,
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
            Status = WebhookLogStatus.Received
        };
    }

    public void MarkProcessed()
    {
        Status = WebhookLogStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = WebhookLogStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }

    public WebhookLogDto ToDto()
    {
        return new WebhookLogDto(
            Id,
            WebhookRegistrationId,
            EventType,
            Status.ToString(),
            ReceivedAt,
            ProcessedAt);
    }
}

internal enum WebhookLogStatus
{
    Received,
    Processed,
    Failed
}
