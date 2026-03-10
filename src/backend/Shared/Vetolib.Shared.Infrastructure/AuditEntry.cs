namespace Vetolib.Shared.Infrastructure;

/// <summary>
/// Represents a single audit log entry capturing a Create, Update, or Delete
/// on any business entity. Stored in the shared.audit_log table.
/// </summary>
public class AuditEntry
{
    public long Id { get; set; }

    /// <summary>Simple class name of the entity, e.g. "Appointment", "Invoice".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The entity primary key as a string.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>"Created", "Updated", or "Deleted".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Email of the authenticated user who triggered the change. Null for system operations.</summary>
    public string? ChangedBy { get; set; }

    /// <summary>Tenant isolation — same ClinicId as the modified entity.</summary>
    public Guid ClinicId { get; set; }

    public DateTime Timestamp { get; set; }

    /// <summary>JSON snapshot of property values before the change. Null for Created.</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON snapshot of property values after the change. Null for Deleted.</summary>
    public string? NewValues { get; set; }
}
