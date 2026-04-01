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
using Vetolib.Auth.Application.Commands.VerifyEmail;
using Vetolib.Auth.Application.Queries.GetCurrentUser;
using Vetolib.Auth.Application.Queries.ListMyOrganizations;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class AuthEndpoints
{
    internal static IEndpointRouteBuilder MapAuthApiEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .RequireRateLimiting("api");

        publicGroup.MapPost("/login", Login)
            .WithName("Login")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Authenticate a user")
            .WithDescription("Validates email and password credentials and returns JWT access and refresh tokens.");

        publicGroup.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Refresh access token")
            .WithDescription("Exchanges a valid refresh token for a new JWT access token and refresh token pair.");

        publicGroup.MapPost("/verify-email", VerifyEmail)
            .WithName("VerifyEmail")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Verify user email address")
            .WithDescription("Verifies a user's email address using the token sent to their email during registration. Sets EmailVerified to true.");

        var authGroup = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .RequireAuthorization()
            .RequireRateLimiting("api");

        authGroup.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Log out the current user")
            .WithDescription("Invalidates the current user's refresh token, effectively ending the session.");

        authGroup.MapGet("/me", GetMe)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user profile")
            .WithDescription("Returns the profile of the currently authenticated user including role and clinic information.");

        authGroup.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .RequireRateLimiting("auth")
            .WithSummary("Change user password")
            .WithDescription("Allows the authenticated user to change their password by providing the current and new passwords.");

        authGroup.MapGet("/my-organizations", ListMyOrganizations)
            .WithName("ListMyOrganizations")
            .WithSummary("List organizations the current user belongs to")
            .WithDescription("Returns all Keycloak organizations (clinics) the authenticated user is a member of. Falls back to ClinicGroup membership when Keycloak is not available.");

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

    private static async Task<Microsoft.AspNetCore.Http.IResult> VerifyEmail(
        string token,
        ISender sender)
        => (await sender.Send(new VerifyEmailCommand(token)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListMyOrganizations(
        ClaimsPrincipal user,
        ISender sender)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Result<IReadOnlyList<MyOrganizationDto>>.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new ListMyOrganizationsQuery(userId))).ToMinimalApiResult();
    }
}
