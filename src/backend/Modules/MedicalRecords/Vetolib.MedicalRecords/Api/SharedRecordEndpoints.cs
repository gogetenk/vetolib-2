using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using IResult = Microsoft.AspNetCore.Http.IResult;
using Vetolib.MedicalRecords.Application.Commands.CreateShareLink;
using Vetolib.MedicalRecords.Application.Commands.RevokeShareLink;
using Vetolib.MedicalRecords.Application.Queries.GetSharedRecord;
using Vetolib.MedicalRecords.Application.Queries.ListShareLinks;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class SharedRecordEndpoints
{
    internal static IEndpointRouteBuilder MapSharedRecordEndpoints(this IEndpointRouteBuilder app)
    {
        // Public endpoint — no auth required
        var publicGroup = app.MapGroup("/api/v1/shared")
            .WithTags("SharedRecords")
            .RequireRateLimiting("api");

        publicGroup.MapGet("/{token}", GetSharedRecord)
            .AllowAnonymous()
            .WithName("GetSharedRecord")
            .WithSummary("View a shared medical record")
            .WithDescription("Returns a medical summary for a patient using a secure share token. No authentication required. Token expires after 72 hours.");

        // Owner-authenticated endpoints
        var portalGroup = app.MapGroup("/api/v1/portal")
            .WithTags("SharedRecords")
            .RequireAuthorization()
            .RequireRateLimiting("api");

        portalGroup.MapPost("/animals/{id:guid}/share", CreateShareLink)
            .WithName("CreateShareLink")
            .WithSummary("Create a share link for a patient")
            .WithDescription("Generates a secure, time-limited share link for a patient's medical summary. The link contains a unique token valid for 72 hours.");

        portalGroup.MapGet("/shares", ListShareLinks)
            .WithName("ListShareLinks")
            .WithSummary("List active share links")
            .WithDescription("Returns all active (not expired, not revoked) share links for the authenticated owner.");

        portalGroup.MapDelete("/shares/{id:guid}", RevokeShareLink)
            .WithName("RevokeShareLink")
            .WithSummary("Revoke a share link")
            .WithDescription("Immediately revokes a share link, preventing further access via its token.");

        return app;
    }

    private static async Task<IResult> GetSharedRecord(
        string token,
        ISender sender)
    {
        return (await sender.Send(new GetSharedRecordQuery(token))).ToMinimalApiResult();
    }

    private static async Task<IResult> CreateShareLink(
        Guid id,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var ownerAccountId = ExtractOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new CreateShareLinkCommand(
            clinicContext.ClinicId,
            id,
            ownerAccountId.Value))).ToMinimalApiResult();
    }

    private static async Task<IResult> ListShareLinks(
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = ExtractOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new ListShareLinksQuery(ownerAccountId.Value))).ToMinimalApiResult();
    }

    private static async Task<IResult> RevokeShareLink(
        Guid id,
        ClaimsPrincipal user,
        ISender sender)
    {
        var ownerAccountId = ExtractOwnerAccountId(user);
        if (ownerAccountId is null)
            return Ardalis.Result.Result.Unauthorized().ToMinimalApiResult();

        return (await sender.Send(new RevokeShareLinkCommand(id, ownerAccountId.Value))).ToMinimalApiResult();
    }

    private static Guid? ExtractOwnerAccountId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("owner_account_id")?.Value
            ?? user.FindFirst("sub")?.Value;

        return claim is not null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
