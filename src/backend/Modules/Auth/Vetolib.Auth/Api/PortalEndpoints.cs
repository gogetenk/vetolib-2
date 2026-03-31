using System.Security.Claims;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.InviteVet;
using Vetolib.Auth.Application.Commands.LinkOwnerByMicrochip;
using Vetolib.Auth.Application.Commands.OwnerPortalLogin;
using Vetolib.Auth.Application.Commands.RegisterOwnerAccount;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class PortalEndpoints
{
    internal static IEndpointRouteBuilder MapPortalApiEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireRateLimiting("api");

        publicGroup.MapPost("/register", Register)
            .WithName("RegisterOwnerAccount")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Register a new pet owner account")
            .WithDescription("Creates a global OwnerAccount and auto-links to existing owner records across all clinics by email or phone.");

        publicGroup.MapPost("/invite-vet", InviteVet)
            .WithName("InviteVet")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Ask your vet to join Vetara")
            .WithDescription("Sends a viral invitation email to a veterinarian on behalf of a pet owner. Rate limited to 3 invitations per vet email per day.");

        publicGroup.MapPost("/login", PortalLogin)
            .WithName("OwnerPortalLogin")
            .AllowAnonymous()
            .RequireRateLimiting("auth")
            .WithSummary("Pet owner portal login")
            .WithDescription("Authenticates an owner account and returns a JWT with ownerAccountId and linkedClinicIds.");

        var authGroup = app.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization()
            .RequireRateLimiting("api");

        authGroup.MapPost("/link-microchip", LinkByMicrochip)
            .WithName("LinkOwnerByMicrochip")
            .WithSummary("Link owner account to patient by microchip")
            .WithDescription("Searches all clinics for a patient with the given microchip number and links its owner to the authenticated owner account.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> InviteVet(
        InviteVetRequest request,
        ISender sender)
        => (await sender.Send(new InviteVetCommand(
            request.VetEmail, request.OwnerName, request.PetName, request.Message)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> Register(
        RegisterOwnerAccountRequest request,
        ISender sender)
        => (await sender.Send(new RegisterOwnerAccountCommand(
            request.Email, request.Phone, request.FullName, request.Password)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> PortalLogin(
        OwnerPortalLoginRequest request,
        ISender sender)
        => (await sender.Send(new OwnerPortalLoginCommand(request.Email, request.Password)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> LinkByMicrochip(
        LinkOwnerByMicrochipRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountIdClaim = user.FindFirst("owner_account_id")?.Value
            ?? user.FindFirst("sub")?.Value;

        if (ownerAccountIdClaim is null || !Guid.TryParse(ownerAccountIdClaim, out var ownerAccountId))
            return Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new LinkOwnerByMicrochipCommand(ownerAccountId, request.MicrochipNumber)))
            .ToMinimalApiResult();
    }
}
