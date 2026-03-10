using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Api.Audit;

internal static class AuditEndpoints
{
    internal static IEndpointRouteBuilder MapAuditApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit")
            .WithTags("Audit")
            .RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)))
            .RequireRateLimiting("api");

        group.MapGet("/", GetAuditLog)
            .WithName("GetAuditLog")
            .WithSummary("Query the audit trail (Create/Update/Delete). Accessible by ADMIN only.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetAuditLog(
        AuditDbContext db,
        IClinicContext clinicContext,
        string? entityType = null,
        string? entityId = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 50;

        // C-03: Filter audit entries to the current clinic — prevents cross-tenant data leakage.
        var clinicId = clinicContext.ClinicId;
        IQueryable<AuditEntry> query = db.AuditLog.AsNoTracking()
            .Where(a => a.ClinicId == clinicId);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value.ToUniversalTime());

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value.ToUniversalTime());

        var totalCount = await query.CountAsync();

        var entries = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditEntryDto(
                a.Id,
                a.EntityType,
                a.EntityId,
                a.Action,
                a.ChangedBy,
                a.ClinicId,
                a.Timestamp,
                a.OldValues,
                a.NewValues))
            .ToListAsync();

        var response = new AuditLogResponse(
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            Entries: entries);

        return Result<AuditLogResponse>.Success(response).ToMinimalApiResult();
    }
}

internal record AuditEntryDto(
    long Id,
    string EntityType,
    string EntityId,
    string Action,
    string? ChangedBy,
    Guid ClinicId,
    DateTime Timestamp,
    string? OldValues,
    string? NewValues);

internal record AuditLogResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<AuditEntryDto> Entries);
