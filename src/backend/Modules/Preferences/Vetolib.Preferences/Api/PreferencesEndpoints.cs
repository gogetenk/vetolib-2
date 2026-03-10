using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Preferences.Application.Commands.BulkUpdatePreferences;
using Vetolib.Preferences.Application.Commands.RevokeConsent;
using Vetolib.Preferences.Application.Commands.UpdateClinicDefaults;
using Vetolib.Preferences.Application.Commands.UpdatePreference;
using Vetolib.Preferences.Application.Queries.GetClinicDefaults;
using Vetolib.Preferences.Application.Queries.GetConsentAudit;
using Vetolib.Preferences.Application.Queries.GetUserPreferences;
using Vetolib.Preferences.Application.Queries.GetUserPreferencesByCategory;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Api;

internal static class PreferencesEndpoints
{
    internal static IEndpointRouteBuilder MapPreferenceApiEndpoints(this IEndpointRouteBuilder app)
    {
        // User preferences
        var group = app.MapGroup("/api/preferences")
            .RequireAuthorization()
            .WithTags("Preferences");

        group.MapGet("/", GetUserPreferences)
            .WithName("GetUserPreferences");

        group.MapGet("/audit", GetConsentAudit)
            .WithName("GetConsentAudit");

        group.MapGet("/{category}", GetUserPreferencesByCategory)
            .WithName("GetUserPreferencesByCategory");

        group.MapPut("/{key}", UpdatePreference)
            .WithName("UpdatePreference");

        group.MapPut("/", BulkUpdatePreferences)
            .WithName("BulkUpdatePreferences");

        group.MapPost("/consent/revoke", RevokeConsent)
            .WithName("RevokeConsent");

        // Clinic-level preferences (admin only)
        var clinicGroup = app.MapGroup("/api/clinics/preferences")
            .RequireAuthorization()
            .WithTags("Preferences");

        clinicGroup.MapGet("/", GetClinicDefaults)
            .WithName("GetClinicDefaults");

        clinicGroup.MapPut("/", UpdateClinicDefaults)
            .WithName("UpdateClinicDefaults");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetUserPreferences(
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var userId = GetCurrentUserId(user);
        return (await sender.Send(new GetUserPreferencesQuery(userId))).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetUserPreferencesByCategory(
        string category,
        ClaimsPrincipal user,
        ISender sender)
    {
        if (!Enum.TryParse<PreferenceCategory>(category, ignoreCase: true, out var categoryEnum))
            return (Ardalis.Result.Result<List<PreferenceDto>>.Invalid(
                new Ardalis.Result.ValidationError("category", $"'{category}' is not a valid preference category")))
                .ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        return (await sender.Send(new GetUserPreferencesByCategoryQuery(userId, categoryEnum))).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdatePreference(
        string key,
        UpdatePreferenceRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        HttpContext httpContext,
        ISender sender)
    {
        if (!Enum.TryParse<PreferenceKey>(key, ignoreCase: true, out var keyEnum))
            return (Ardalis.Result.Result.Invalid(
                new Ardalis.Result.ValidationError("key", $"'{key}' is not a valid preference key")))
                .ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();

        var cmd = new UpdatePreferenceCommand(
            clinicContext.ClinicId,
            userId,
            keyEnum,
            request.Value,
            ipAddress,
            userAgent);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> BulkUpdatePreferences(
        BulkUpdatePreferencesRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        HttpContext httpContext,
        ISender sender)
    {
        var userId = GetCurrentUserId(user);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();

        var items = request.Preferences
            .Select(p => new PreferenceUpdateItem(p.Key, p.Value))
            .ToList()
            .AsReadOnly();

        var cmd = new BulkUpdatePreferencesCommand(
            clinicContext.ClinicId,
            userId,
            items,
            ipAddress,
            userAgent);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RevokeConsent(
        RevokeConsentRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        HttpContext httpContext,
        ISender sender)
    {
        var userId = GetCurrentUserId(user);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.FirstOrDefault();

        var cmd = new RevokeConsentCommand(
            clinicContext.ClinicId,
            userId,
            request.Category,
            ipAddress,
            userAgent);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetClinicDefaults(
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<List<PreferenceCategoryDto>>.Forbidden()).ToMinimalApiResult();

        return (await sender.Send(new GetClinicDefaultsQuery())).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> UpdateClinicDefaults(
        UpdateClinicDefaultsRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result.Forbidden()).ToMinimalApiResult();

        var userId = GetCurrentUserId(user);

        var defaults = request.Defaults
            .Select(d => new ClinicDefaultItem(d.Key, d.Value))
            .ToList()
            .AsReadOnly();

        var cmd = new UpdateClinicDefaultsCommand(clinicContext.ClinicId, userId, defaults);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetConsentAudit(
        ClaimsPrincipal user,
        [AsParameters] ConsentAuditQueryParams queryParams,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<ConsentAuditPagedResultDto>.Forbidden()).ToMinimalApiResult();

        PreferenceCategory? category = null;
        if (!string.IsNullOrEmpty(queryParams.Category) &&
            Enum.TryParse<PreferenceCategory>(queryParams.Category, ignoreCase: true, out var cat))
            category = cat;

        var query = new GetConsentAuditQuery(
            queryParams.UserId,
            category,
            queryParams.From,
            queryParams.To,
            queryParams.Page,
            queryParams.PageSize);

        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

internal record ConsentAuditQueryParams(
    Guid? UserId = null,
    string? Category = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20);
