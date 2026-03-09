using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.CreateUser;
using Vetolib.Auth.Application.Queries.GetUsers;
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

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreateUser(
        CreateUserRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        // Check Admin role
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

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetUsers(
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin")
            return (Ardalis.Result.Result<IReadOnlyList<UserDto>>.Forbidden()).ToMinimalApiResult();

        return (await sender.Send(new GetUsersQuery())).ToMinimalApiResult();
    }
}
