using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Messaging.Application.Commands.AcceptConsent;
using Vetolib.Messaging.Application.Commands.CreateOwnerConversation;
using Vetolib.Messaging.Application.Commands.SendOwnerMessage;
using Vetolib.Messaging.Application.Queries.ExportOwnerConversations;
using Vetolib.Messaging.Application.Queries.GetOwnerConversationById;
using Vetolib.Messaging.Application.Queries.ListOwnerConversations;
using Vetolib.Messaging.Application.Queries.ListOwnerPets;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Api;

internal static class PortalEndpoints
{
    internal static IEndpointRouteBuilder MapPortalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/portal")
            .AddEndpointFilter<MagicLinkEndpointFilter>()
            .WithTags("OwnerPortal");

        // GET /conversations — list owner's conversations
        group.MapGet("/conversations", async (
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ListOwnerConversationsQuery(portal.OwnerId, portal.ClinicId), ct);
            return result.ToMinimalApiResult();
        });

        // GET /conversations/{id} — get conversation with messages (no internal notes)
        group.MapGet("/conversations/{id:guid}", async (
            Guid id,
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetOwnerConversationByIdQuery(id, portal.OwnerId, portal.ClinicId), ct);
            return result.ToMinimalApiResult();
        });

        // POST /conversations — create new conversation
        group.MapPost("/conversations", async (
            CreateOwnerConversationRequest request,
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateOwnerConversationCommand(
                    portal.OwnerId,
                    portal.ClinicId,
                    request.PatientId,
                    request.Subject,
                    request.Category,
                    request.Body), ct);
            return result.ToMinimalApiResult();
        });

        // POST /conversations/{id}/messages — send a message in existing conversation
        group.MapPost("/conversations/{id:guid}/messages", async (
            Guid id,
            SendOwnerMessageRequest request,
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SendOwnerMessageCommand(
                    id,
                    portal.OwnerId,
                    portal.ClinicId,
                    request.Body), ct);
            return result.ToMinimalApiResult();
        });

        // POST /consent — record consent acceptance
        group.MapPost("/consent", async (
            AcceptConsentRequest request,
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new AcceptConsentCommand(
                    portal.OwnerId,
                    portal.ClinicId,
                    request.ConsentVersion), ct);
            return result.ToMinimalApiResult();
        });

        // GET /export — download all conversations as text (PDPL right of access)
        group.MapGet("/export", async (
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ExportOwnerConversationsQuery(portal.OwnerId, portal.ClinicId), ct);

            if (!result.IsSuccess)
                return result.ToMinimalApiResult();

            return Results.Text(
                result.Value,
                "text/plain",
                System.Text.Encoding.UTF8,
                200);
        });

        // GET /pets — list owner's registered pets
        group.MapGet("/pets", async (
            IPortalContext portal,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new ListOwnerPetsQuery(portal.OwnerId, portal.ClinicId), ct);
            return result.ToMinimalApiResult();
        });

        return app;
    }
}

// Request DTOs for this module
internal record CreateOwnerConversationRequest(
    Guid? PatientId,
    string Subject,
    Vetolib.Messaging.Contracts.MessageCategory Category,
    string Body
);

internal record SendOwnerMessageRequest(string Body);

internal record AcceptConsentRequest(string ConsentVersion);
