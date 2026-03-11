using System.Security.Claims;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.ChangePassword;
using Vetolib.Auth.Application.Commands.Login;
using Vetolib.Auth.Application.Commands.Logout;
using Vetolib.Auth.Application.Commands.RefreshToken;
using Vetolib.Auth.Application.Queries.GetCurrentUser;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class AuthEndpoints
{
    internal static IEndpointRouteBuilder MapAuthApiEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        publicGroup.MapPost("/login", Login)
            .WithName("Login")
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        publicGroup.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        var authGroup = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .RequireAuthorization();

        authGroup.MapPost("/logout", Logout)
            .WithName("Logout");

        authGroup.MapGet("/me", GetMe)
            .WithName("GetCurrentUser");

        authGroup.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .RequireRateLimiting("auth");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> Login(
        LoginRequest request,
        ISender sender)
        => (await sender.Send(new LoginCommand(request.Email, request.Password)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> Refresh(
        RefreshTokenRequest request,
        ISender sender)
        => (await sender.Send(new RefreshTokenCommand(request.RefreshToken)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> Logout(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new LogoutCommand(userId))).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetMe(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result<UserDto>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new GetCurrentUserQuery(userId))).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ChangePassword(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword)))
            .ToMinimalApiResult();
    }
}
