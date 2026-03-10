using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

/// <summary>
/// EF Core SaveChanges interceptor that writes an AuditEntry for every
/// Create / Update / Delete on business entities (BaseEntity subclasses).
///
/// Entries are captured before save (to read OriginalValues) and persisted
/// after the save via a dedicated AuditDbContext scope, keeping the audit
/// write outside the original transaction.
///
/// RefreshToken is explicitly excluded (auth tokens are handled separately).
///
/// Registration: call services.AddSingleton&lt;AuditSaveChangesInterceptor&gt;()
/// then wire it per DbContext via AddAuditInterceptor&lt;T&gt;().
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;

    // Type name to exclude from auditing entirely.
    private const string RefreshTokenTypeName = "RefreshToken";

    // Property names to redact from OldValues / NewValues JSON (H-04).
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "SecurityStamp",
        "RefreshToken"
    };

    // Per-call state — keyed by context instance identity hash (RuntimeHelpers.GetHashCode)
    // to avoid collisions from overridden GetHashCode implementations.
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, List<AuditEntry>>
        _pendingEntries = new();

    public AuditSaveChangesInterceptor(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = scopeFactory;
    }

    // ── Before save: capture entry state (OriginalValues available here) ──────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return new ValueTask<InterceptionResult<int>>(result);

        var auditEntries = CaptureAuditEntries(eventData.Context);
        if (auditEntries.Count > 0)
            _pendingEntries[RuntimeHelpers.GetHashCode(eventData.Context)] = auditEntries;

        return new ValueTask<InterceptionResult<int>>(result);
    }

    // ── After save: persist captured entries via dedicated AuditDbContext ─────

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return result;

        var key = RuntimeHelpers.GetHashCode(eventData.Context);
        if (!_pendingEntries.TryRemove(key, out var entries) || entries.Count == 0)
            return result;

        using var scope = _scopeFactory.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        auditDb.AuditLog.AddRange(entries);
        await auditDb.SaveChangesAsync(cancellationToken);

        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private List<AuditEntry> CaptureAuditEntries(DbContext context)
    {
        var changedBy = GetCurrentUserEmail();
        var clinicId = context is MultiTenantDbContext mtCtx
            ? mtCtx.ClinicContext.ClinicId
            : Guid.Empty;
        var now = DateTime.UtcNow;

        var entries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var typeName = entry.Entity.GetType().Name;

            // Exclude RefreshToken from audit trail.
            if (typeName == RefreshTokenTypeName)
                continue;

            var action = entry.State switch
            {
                EntityState.Added    => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted  => "Deleted",
                _                    => "Unknown"
            };

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var oldProps = new Dictionary<string, object?>();
                var newProps = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !SensitiveProperties.Contains(prop.Metadata.Name))
                    {
                        oldProps[prop.Metadata.Name] = prop.OriginalValue;
                        newProps[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                if (oldProps.Count > 0)
                {
                    oldValues = SerializeValues(oldProps);
                    newValues = SerializeValues(newProps);
                }
            }
            else if (entry.State == EntityState.Added)
            {
                var props = entry.Properties
                    .Where(p => !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = SerializeValues(props);
            }
            else // Deleted
            {
                var props = entry.Properties
                    .Where(p => !SensitiveProperties.Contains(p.Metadata.Name))
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                oldValues = SerializeValues(props);
            }

            // Use the entity's own ClinicId when available (preferred),
            // falling back to the ambient ClinicContext.
            var entityClinicId = entry.Entity is IMultiTenant mt && mt.ClinicId != Guid.Empty
                ? mt.ClinicId
                : clinicId;

            entries.Add(new AuditEntry
            {
                EntityType = typeName,
                EntityId   = entry.Entity.Id.ToString(),
                Action     = action,
                ChangedBy  = changedBy,
                ClinicId   = entityClinicId,
                Timestamp  = now,
                OldValues  = oldValues,
                NewValues  = newValues
            });
        }

        return entries;
    }

    private string? GetCurrentUserEmail()
    {
        var http = _httpContextAccessor.HttpContext;
        if (http is null) return null;

        return http.User.FindFirst("email")?.Value
            ?? http.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? http.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }

    private static string SerializeValues(Dictionary<string, object?> values)
    {
        var serializable = values.ToDictionary(
            kv => kv.Key,
            kv => kv.Value?.ToString());
        return JsonSerializer.Serialize(serializable);
    }
}
