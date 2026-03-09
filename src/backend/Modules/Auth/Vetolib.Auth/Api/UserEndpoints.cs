using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.ChangeUserRole;
using Vetolib.Auth.Application.Commands.CreateUser;
using Vetolib.Auth.Application.Commands.DeactivateUser;
using Vetolib.Auth.Application.Commands.InviteUser;
using Vetolib.Auth.Application.Queries.ListUsers;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Api;

internal static class UserEndpoints
{
    internal static IEndpointRouteBuilder MapUserApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .RequireAuthorization()
            .WithTags("Users");

        group.MapPost("/", CreateUser)
            .WithName("CreateUser");

        group.MapGet("/", GetUsers)
            .WithName("GetUsers");

        group.MapPost("/invite", InviteUser)
            .WithName("InviteUser");

        group.MapPatch("/{id:guid}/role", ChangeRole)
            .WithName("ChangeUserRole");

        group.MapDelete("/{id:guid}", DeactivateUser)
            .WithName("DeactivateUser");

        return app;
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<UserDto>.Forbidden()).ToMinimalApiResult();

        var cmd = new CreateUserCommand(
            clinicContext.ClinicId,
            request.Email,
            request.Password,
            request.Role,
            request.VetLicenseNumber);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetUsers(
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<IReadOnlyList<UserListItemDto>>.Forbidden()).ToMinimalApiResult();

        return (await sender.Send(new ListUsersQuery())).ToMinimalApiResult();
    }

    private static async Task<IResult> InviteUser(
        InviteUserRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<InviteUserResponse>.Forbidden()).ToMinimalApiResult();

        var requestingUserId = GetCurrentUserId(user);
        var cmd = new InviteUserCommand(
            clinicContext.ClinicId,
            requestingUserId,
            request.Email,
            request.FullName,
            request.Role);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ChangeRole(
        Guid id,
        ChangeRoleRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result.Forbidden()).ToMinimalApiResult();

        var requestingUserId = GetCurrentUserId(user);
        var cmd = new ChangeUserRoleCommand(requestingUserId, id, request.NewRole);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> DeactivateUser(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result.Forbidden()).ToMinimalApiResult();

        var requestingUserId = GetCurrentUserId(user);
        var cmd = new DeactivateUserCommand(requestingUserId, id);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
