using System.Security.Claims;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.AddClinicToGroup;
using Vetolib.Auth.Application.Commands.CreateClinicGroup;
using Vetolib.Auth.Application.Commands.RemoveClinicFromGroup;
using Vetolib.Auth.Application.Commands.SwitchClinic;
using Vetolib.Auth.Application.Queries.GetGroupClinicStats;
using Vetolib.Auth.Application.Queries.GetGroupDashboardStats;
using Vetolib.Auth.Application.Queries.GetGroupRevenueComparison;
using Vetolib.Auth.Application.Queries.ListGroupClinics;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class ClinicGroupEndpoints
{
    internal static IEndpointRouteBuilder MapClinicGroupApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clinic-groups")
            .RequireAuthorization()
            .WithTags("ClinicGroups");

        group.MapPost("/", CreateClinicGroup)
            .WithName("CreateClinicGroup")
            .WithSummary("Create a clinic group")
            .WithDescription("Creates a new clinic group for multi-clinic management. Requires Admin role.");

        group.MapPost("/{id:guid}/clinics", AddClinicToGroup)
            .WithName("AddClinicToGroup")
            .WithSummary("Add a clinic to a group")
            .WithDescription("Associates an existing clinic with a clinic group. Requires Admin role.");

        group.MapGet("/{id:guid}/clinics", ListGroupClinics)
            .WithName("ListGroupClinics")
            .WithSummary("List clinics in a group")
            .WithDescription("Returns all clinics that belong to the specified clinic group.");

        group.MapDelete("/{id:guid}/clinics/{clinicId:guid}", RemoveClinicFromGroup)
            .WithName("RemoveClinicFromGroup")
            .WithSummary("Remove a clinic from a group")
            .WithDescription("Removes a clinic from a clinic group. Requires Admin role.");

        // Dashboard endpoints
        group.MapGet("/{id:guid}/dashboard/stats", GetGroupDashboardStats)
            .WithName("GetGroupDashboardStats")
            .WithSummary("Get aggregated dashboard stats for a clinic group")
            .WithDescription("Returns total patients, appointments, and revenue across all clinics in the group. Requires Admin role and group ownership.");

        group.MapGet("/{id:guid}/dashboard/clinics", GetGroupClinicStats)
            .WithName("GetGroupClinicStats")
            .WithSummary("List clinics in a group with individual stats")
            .WithDescription("Returns per-clinic stats (patients, appointments, revenue) for all clinics in the group. Requires Admin role and group ownership.");

        group.MapGet("/{id:guid}/dashboard/revenue-comparison", GetGroupRevenueComparison)
            .WithName("GetGroupRevenueComparison")
            .WithSummary("Compare revenue across clinics in a group")
            .WithDescription("Returns revenue per clinic for comparison within the group. Requires Admin role and group ownership.");

        // Switch clinic endpoint under /api/v1/auth
        var authGroup = app.MapGroup("/api/v1/auth")
            .RequireAuthorization()
            .WithTags("Auth");

        authGroup.MapPost("/switch-clinic", SwitchClinic)
            .WithName("SwitchClinic")
            .WithSummary("Switch active clinic")
            .WithDescription("Switches the authenticated user's active clinic context and returns new tokens scoped to the target clinic.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateClinicGroup(
        CreateClinicGroupRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result<ClinicGroupDto>.Forbidden().ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
            return Result<ClinicGroupDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new CreateClinicGroupCommand(request.Name, userId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> AddClinicToGroup(
        Guid id,
        AddClinicToGroupRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new AddClinicToGroupCommand(id, request.ClinicId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListGroupClinics(
        Guid id,
        ISender sender)
        => (await sender.Send(new ListGroupClinicsQuery(id))).ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> RemoveClinicFromGroup(
        Guid id,
        Guid clinicId,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result.Forbidden().ToMinimalApiResult();

        return (await sender.Send(new RemoveClinicFromGroupCommand(id, clinicId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> SwitchClinic(
        SwitchClinicRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
            return Result<AuthTokenDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new SwitchClinicCommand(userId, request.ClinicId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetGroupDashboardStats(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result<ClinicGroupDashboardStatsDto>.Forbidden().ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
            return Result<ClinicGroupDashboardStatsDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetGroupDashboardStatsQuery(id, userId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetGroupClinicStats(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Forbidden().ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
            return Result<IReadOnlyList<ClinicGroupClinicStatsDto>>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetGroupClinicStatsQuery(id, userId)))
            .ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetGroupRevenueComparison(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return Result<ClinicGroupRevenueComparisonDto>.Forbidden().ToMinimalApiResult();

        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
            return Result<ClinicGroupRevenueComparisonDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetGroupRevenueComparisonQuery(id, userId)))
            .ToMinimalApiResult();
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
