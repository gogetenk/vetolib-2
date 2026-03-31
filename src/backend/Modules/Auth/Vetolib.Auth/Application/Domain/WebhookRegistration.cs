using Ardalis.Result;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class WebhookRegistration : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Secret { get; private set; } = string.Empty;

    private readonly List<string> _eventTypes = [];
    public IReadOnlyList<string> EventTypes => _eventTypes.AsReadOnly();

    public bool IsActive { get; private set; } = true;
    public DateTime? LastCalledAt { get; private set; }

    private readonly List<WebhookLog> _logs = [];
    public IReadOnlyCollection<WebhookLog> Logs => _logs.AsReadOnly();

    private WebhookRegistration() { } // EF Core constructor

    public static Result<WebhookRegistration> Create(Guid clinicId, string name, string secret, List<string> eventTypes)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));

        if (string.IsNullOrWhiteSpace(secret))
            errors.Add(new ValidationError(nameof(secret), "Secret is required"));

        if (secret is not null && secret.Length < 16)
            errors.Add(new ValidationError(nameof(secret), "Secret must be at least 16 characters"));

        if (eventTypes is null || eventTypes.Count == 0)
            errors.Add(new ValidationError(nameof(eventTypes), "At least one event type is required"));

        var allowedTypes = new HashSet<string> { "lab.result", "external.record" };
        if (eventTypes is not null)
        {
            var invalid = eventTypes.Where(t => !allowedTypes.Contains(t)).ToList();
            if (invalid.Count > 0)
                errors.Add(new ValidationError(nameof(eventTypes), $"Invalid event types: {string.Join(", ", invalid)}"));
        }

        if (errors.Count > 0)
            return Result<WebhookRegistration>.Invalid(errors);

        var registration = new WebhookRegistration
        {
            ClinicId = clinicId,
            Name = name.Trim(),
            Secret = secret,
        };
        registration._eventTypes.AddRange(eventTypes!);

        return Result<WebhookRegistration>.Success(registration);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Error("Webhook registration is already inactive");

        IsActive = false;
        return Result.Success();
    }

    public void RecordCall()
    {
        LastCalledAt = DateTime.UtcNow;
    }

    public bool SupportsEventType(string eventType)
    {
        return _eventTypes.Contains(eventType);
    }

    public WebhookRegistrationDto ToDto()
    {
        return new WebhookRegistrationDto(
            Id,
            Name,
            _eventTypes.ToList(),
            IsActive,
            CreatedAt,
            LastCalledAt);
    }
}
