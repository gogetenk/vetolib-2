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
            .WithName("CreateClinicGroup");

        group.MapPost("/{id:guid}/clinics", AddClinicToGroup)
            .WithName("AddClinicToGroup");

        group.MapGet("/{id:guid}/clinics", ListGroupClinics)
            .WithName("ListGroupClinics");

        group.MapDelete("/{id:guid}/clinics/{clinicId:guid}", RemoveClinicFromGroup)
            .WithName("RemoveClinicFromGroup");

        // Switch clinic endpoint under /api/v1/auth
        var authGroup = app.MapGroup("/api/v1/auth")
            .RequireAuthorization()
            .WithTags("Auth");

        authGroup.MapPost("/switch-clinic", SwitchClinic)
            .WithName("SwitchClinic");

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

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
