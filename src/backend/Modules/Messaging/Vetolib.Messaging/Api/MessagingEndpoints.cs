using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Messaging.Application.Commands.AddInternalNote;
using Vetolib.Messaging.Application.Commands.ChangeConversationStatus;
using Vetolib.Messaging.Application.Commands.CreateOutboundConversation;
using Vetolib.Messaging.Application.Commands.CreateTemplate;
using Vetolib.Messaging.Application.Commands.DeleteTemplate;
using Vetolib.Messaging.Application.Commands.MarkAsSpam;
using Vetolib.Messaging.Application.Commands.RecategorizeConversation;
using Vetolib.Messaging.Application.Commands.SendReply;
using Vetolib.Messaging.Application.Commands.TransferConversation;
using Vetolib.Messaging.Application.Commands.UpdateMessagingHours;
using Vetolib.Messaging.Application.Commands.UpdateTemplate;
using Vetolib.Messaging.Application.Queries.GetConversationById;
using Vetolib.Messaging.Application.Queries.GetConversationSummary;
using Vetolib.Messaging.Application.Queries.GetMessagingHours;
using Vetolib.Messaging.Application.Queries.GetTriageStats;
using Vetolib.Messaging.Application.Queries.ListConversations;
using Vetolib.Messaging.Application.Queries.ListTemplates;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Api;

internal static class MessagingEndpoints
{
    private const string AdminRole = "Admin";

    internal static IEndpointRouteBuilder MapMessagingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/messaging")
            .RequireAuthorization()
            .WithTags("Messaging");

        // -----------------------------------------------------------------------
        // Staff: Conversations
        // -----------------------------------------------------------------------

        // GET /conversations — list conversations filtered by role
        group.MapGet("/conversations", async (
            ISender sender,
            ConversationStatus? status,
            MessageCategory? category,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = new ListConversationsQuery(status, category, fromDate, toDate, page, pageSize);
            return (await sender.Send(query, ct)).ToMinimalApiResult();
        }).WithName("ListConversations");

        // GET /conversations/{id} — get conversation with messages
        group.MapGet("/conversations/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetConversationByIdQuery(id), ct)).ToMinimalApiResult()
        ).WithName("GetConversationById");

        // POST /conversations/{id}/reply — send reply (ClinicStaff, not Assistant)
        group.MapPost("/conversations/{id:guid}/reply", async (
            Guid id,
            StaffReplyRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new SendReplyCommand(id, request.Body, request.AiSuggestedReply, request.WasSuggestedReplyUsed);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("SendReply");

        // POST /conversations/{id}/notes — add internal note (VetOrAdmin)
        group.MapPost("/conversations/{id:guid}/notes", async (
            Guid id,
            InternalNoteRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new AddInternalNoteCommand(id, request.Body);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("AddInternalNote");

        // PATCH /conversations/{id}/status — change status (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/status", async (
            Guid id,
            ConversationStatusChangeRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new ChangeConversationStatusCommand(id, request.Action);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("ChangeConversationStatus");

        // PATCH /conversations/{id}/transfer — transfer conversation (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/transfer", async (
            Guid id,
            ConversationTransferRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new TransferConversationCommand(id, request.AssignedToUserId, request.AssignedToRole);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("TransferConversation");

        // PATCH /conversations/{id}/category — re-categorize (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/category", async (
            Guid id,
            ConversationRecategorizeRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new RecategorizeConversationCommand(id, request.NewCategory);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("RecategorizeConversation");

        // POST /conversations/{id}/spam — mark as spam (ClinicStaff)
        group.MapPost("/conversations/{id:guid}/spam", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkAsSpamCommand(id), ct)).ToMinimalApiResult()
        ).WithName("MarkAsSpam");

        // POST /conversations/outbound — create proactive conversation (AdminOnly)
        group.MapPost("/conversations/outbound", async (
            OutboundConversationRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new CreateOutboundConversationCommand(
                request.OwnerId,
                request.PatientId,
                request.Subject,
                request.InitialMessageBody,
                request.Category);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).RequireAuthorization(policy => policy.RequireRole(AdminRole))
          .WithName("CreateOutboundConversation");

        // GET /conversations/{id}/summary — AI conversation summary (VetOrAdmin)
        group.MapGet("/conversations/{id:guid}/summary", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetConversationSummaryQuery(id), ct)).ToMinimalApiResult()
        ).WithName("GetConversationSummary");

        // -----------------------------------------------------------------------
        // Admin: Settings
        // -----------------------------------------------------------------------

        var settings = group.MapGroup("/settings")
            .RequireAuthorization(policy => policy.RequireRole(AdminRole));

        // GET /api/v1/messaging/settings/hours
        settings.MapGet("/hours", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMessagingHoursQuery(), ct)).ToMinimalApiResult());

        // PUT /api/v1/messaging/settings/hours
        settings.MapPut("/hours", async (UpdateMessagingHoursRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateMessagingHoursCommand(request.Days), ct)).ToMinimalApiResult());

        // GET /api/v1/messaging/stats
        group.MapGet("/stats", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetTriageStatsQuery(), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole));

        // Templates (quick response templates)
        // GET /api/v1/messaging/templates
        group.MapGet("/templates", async (string? category, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListTemplatesQuery(category), ct)).ToMinimalApiResult())
            .RequireAuthorization("ClinicStaff")
            .WithName("ListTemplates");

        // POST /api/v1/messaging/templates
        group.MapPost("/templates", async (CreateTemplateRequest request, IClinicContext clinicContext, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateTemplateCommand(
                clinicContext.ClinicId,
                request.Name,
                request.ContentEn,
                request.ContentAr,
                request.Category), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("CreateTemplate");

        // PUT /api/v1/messaging/templates/{id}
        group.MapPut("/templates/{id:guid}", async (Guid id, UpdateTemplateRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateTemplateCommand(
                id,
                request.Name,
                request.ContentEn,
                request.ContentAr,
                request.Category), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("UpdateTemplate");

        // DELETE /api/v1/messaging/templates/{id}
        group.MapDelete("/templates/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteTemplateCommand(id), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("DeleteTemplate");

        return app;
    }
}
