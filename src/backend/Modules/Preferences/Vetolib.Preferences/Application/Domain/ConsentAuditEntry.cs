using Ardalis.Result;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Domain;

/// <summary>
/// Append-only audit record for consent/preference changes.
/// Never updated or deleted — only created.
/// </summary>
internal class ConsentAuditEntry : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid UserId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public PreferenceKey Key { get; private set; }
    public string? PreviousValue { get; private set; }
    public string NewValue { get; private set; } = string.Empty;
    public PreferenceSource Source { get; private set; }

    private ConsentAuditEntry() { } // EF Core

    public static Result<ConsentAuditEntry> Create(
        Guid clinicId,
        Guid userId,
        PreferenceCategory category,
        PreferenceKey key,
        string? previousValue,
        string newValue,
        PreferenceSource source)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (userId == Guid.Empty)
            errors.Add(new ValidationError(nameof(userId), "UserId is required"));

        if (string.IsNullOrWhiteSpace(newValue))
            errors.Add(new ValidationError(nameof(newValue), "NewValue is required"));

        if (errors.Count > 0)
            return Result<ConsentAuditEntry>.Invalid(errors);

        return Result<ConsentAuditEntry>.Success(new ConsentAuditEntry
        {
            ClinicId = clinicId,
            UserId = userId,
            Category = category,
            Key = key,
            PreviousValue = previousValue,
            NewValue = newValue,
            Source = source
        });
    }

    public ConsentAuditDto ToDto() => new(
        Id,
        UserId,
        Category,
        Key,
        PreviousValue,
        NewValue,
        Source,
        CreatedAt);
}
