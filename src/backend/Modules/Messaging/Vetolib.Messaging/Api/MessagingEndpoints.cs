using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Messaging.Application.Commands.AddInternalNote;
using Vetolib.Messaging.Application.Commands.AddMessageToRecord;
using Vetolib.Messaging.Application.Commands.ChangeConversationStatus;
using Vetolib.Messaging.Application.Commands.ConvertToAppointment;
using Vetolib.Messaging.Application.Commands.CreateOutboundConversation;
using Vetolib.Messaging.Application.Commands.CreateTemplate;
using Vetolib.Messaging.Application.Commands.UploadFiles;
using Vetolib.Messaging.Application.Commands.OverrideClassification;
using Vetolib.Messaging.Application.Commands.ClassificationFeedback;
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
using Vetolib.Messaging.Application.Queries.GetClassificationAccuracy;
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
            .RequireRateLimiting("api")
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
            if (pageSize is < 1 or > 200) pageSize = 20;
            var query = new ListConversationsQuery(status, category, fromDate, toDate, page, pageSize);
            return (await sender.Send(query, ct)).ToMinimalApiResult();
        }).WithName("ListConversations")
          .WithSummary("List conversations")
          .WithDescription("Returns a paginated, filterable list of messaging conversations for the current clinic.");

        // GET /conversations/{id} — get conversation with messages
        group.MapGet("/conversations/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetConversationByIdQuery(id), ct)).ToMinimalApiResult()
        ).WithName("GetConversationById")
          .WithSummary("Get conversation by ID")
          .WithDescription("Returns a conversation with all its messages, including internal notes for staff.");

        // POST /conversations/{id}/reply — send reply (ClinicStaff, not Assistant)
        group.MapPost("/conversations/{id:guid}/reply", async (
            Guid id,
            StaffReplyRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new SendReplyCommand(id, request.Body, request.AiSuggestedReply, request.WasSuggestedReplyUsed, request.AttachmentIds);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("SendReply")
          .WithSummary("Send a reply to a conversation")
          .WithDescription("Sends a staff reply message in an existing conversation. Supports AI-suggested reply tracking.");

        // POST /conversations/{id}/notes — add internal note (VetOrAdmin)
        group.MapPost("/conversations/{id:guid}/notes", async (
            Guid id,
            InternalNoteRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new AddInternalNoteCommand(id, request.Body);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("AddInternalNote")
          .WithSummary("Add an internal note")
          .WithDescription("Adds a private staff-only note to a conversation. Not visible to pet owners.");

        // PATCH /conversations/{id}/status — change status (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/status", async (
            Guid id,
            ConversationStatusChangeRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new ChangeConversationStatusCommand(id, request.Action);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("ChangeConversationStatus")
          .WithSummary("Change conversation status")
          .WithDescription("Updates the status of a conversation (e.g., Open, InProgress, Resolved, Closed).");

        // PATCH /conversations/{id}/transfer — transfer conversation (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/transfer", async (
            Guid id,
            ConversationTransferRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new TransferConversationCommand(id, request.AssignedToUserId, request.AssignedToRole);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("TransferConversation")
          .WithSummary("Transfer a conversation")
          .WithDescription("Reassigns a conversation to a different staff member or role.");

        // PATCH /conversations/{id}/category — re-categorize (ClinicStaff)
        group.MapPatch("/conversations/{id:guid}/category", async (
            Guid id,
            ConversationRecategorizeRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new RecategorizeConversationCommand(id, request.NewCategory);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("RecategorizeConversation")
          .WithSummary("Re-categorize a conversation")
          .WithDescription("Changes the message category of a conversation (e.g., MedicalUrgency, Appointment, General).");

        // POST /conversations/{id}/spam — mark as spam (ClinicStaff)
        group.MapPost("/conversations/{id:guid}/spam", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkAsSpamCommand(id), ct)).ToMinimalApiResult()
        ).WithName("MarkAsSpam")
          .WithSummary("Mark conversation as spam")
          .WithDescription("Flags a conversation as spam, hiding it from the active conversation list.");

        // POST /conversations/{id}/convert-to-appointment — convert to appointment (ClinicStaff)
        group.MapPost("/conversations/{id:guid}/convert-to-appointment", async (
            Guid id,
            ConvertToAppointmentRequest? request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new ConvertToAppointmentCommand(id, request?.PreferredDate, request?.Notes);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).RequireAuthorization("ClinicStaff")
          .WithName("ConvertToAppointment")
          .WithSummary("Convert conversation to appointment")
          .WithDescription("Creates an appointment from a messaging conversation. Requires ClinicStaff authorization.");

        // POST /conversations/{conversationId}/messages/{messageId}/add-to-record — attach message to medical record (VetOrAdmin)
        group.MapPost("/conversations/{conversationId:guid}/messages/{messageId:guid}/add-to-record", async (
            Guid conversationId,
            Guid messageId,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new AddMessageToRecordCommand(conversationId, messageId);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).RequireAuthorization(policy => policy.RequireRole("Vet", AdminRole))
          .WithName("AddMessageToRecord")
          .WithSummary("Attach message to medical record")
          .WithDescription("Links a conversation message to the patient's medical record for clinical documentation. Requires Vet or Admin role.");

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
          .WithName("CreateOutboundConversation")
          .WithSummary("Create an outbound conversation")
          .WithDescription("Initiates a proactive conversation with a pet owner. Requires Admin role.");

        // GET /conversations/{id}/summary — AI conversation summary (VetOrAdmin)
        group.MapGet("/conversations/{id:guid}/summary", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetConversationSummaryQuery(id), ct)).ToMinimalApiResult()
        ).WithName("GetConversationSummary")
          .WithSummary("Get AI conversation summary")
          .WithDescription("Returns an AI-generated summary of the conversation for quick review by veterinary staff.");

        // -----------------------------------------------------------------------
        // Staff: Message Classification
        // -----------------------------------------------------------------------

        // PATCH /conversations/{id}/messages/{messageId}/classify — override classification (Vet/Admin)
        group.MapPatch("/conversations/{id:guid}/messages/{messageId:guid}/classify", async (
            Guid id,
            Guid messageId,
            ClassifyMessageOverrideRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new OverrideClassificationCommand(id, messageId, request.Urgency, request.Category);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).RequireAuthorization(policy => policy.RequireRole("Vet", AdminRole))
          .WithName("OverrideClassification")
          .WithSummary("Override message classification")
          .WithDescription("Manually overrides the AI-assigned urgency and category of a message. Requires Vet or Admin role.");

        // POST /conversations/{id}/messages/{messageId}/classify/feedback — classification feedback
        group.MapPost("/conversations/{id:guid}/messages/{messageId:guid}/classify/feedback", async (
            Guid id,
            Guid messageId,
            ClassificationFeedbackRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new ClassificationFeedbackCommand(id, messageId, request.IsCorrect);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).RequireAuthorization("ClinicStaff")
          .WithName("ClassificationFeedback")
          .WithSummary("Provide classification feedback")
          .WithDescription("Records whether the AI classification of a message was correct, used to improve classification accuracy over time.");

        // GET /stats/classification-accuracy — classification accuracy report (Admin)
        group.MapGet("/stats/classification-accuracy", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetClassificationAccuracyQuery(), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("GetClassificationAccuracy")
            .WithSummary("Get classification accuracy report")
            .WithDescription("Returns accuracy metrics for AI message classification based on staff feedback. Requires Admin role.");

        // -----------------------------------------------------------------------
        // Staff: File Upload
        // -----------------------------------------------------------------------

        // POST /upload — upload files for attachment (max 5 files, max 10MB each, PDF/JPG/PNG only)
        group.MapPost("/upload", async (
            HttpRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType || request.Form.Files.Count == 0)
                return Results.BadRequest("No files provided");

            var cmd = new UploadFilesCommand(request.Form.Files);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).DisableAntiforgery()
          .WithName("UploadFiles")
          .WithSummary("Upload message attachments")
          .WithDescription("Uploads files to attach to messages. Maximum 5 files, 10MB each. Accepts PDF, JPG, and PNG formats.");

        // -----------------------------------------------------------------------
        // Admin: Settings
        // -----------------------------------------------------------------------

        var settings = group.MapGroup("/settings")
            .RequireAuthorization(policy => policy.RequireRole(AdminRole));

        // GET /api/v1/messaging/settings/hours
        settings.MapGet("/hours", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMessagingHoursQuery(), ct)).ToMinimalApiResult())
            .WithSummary("Get messaging hours")
            .WithDescription("Returns the clinic's configured messaging availability hours for each day of the week.");

        // PUT /api/v1/messaging/settings/hours
        settings.MapPut("/hours", async (UpdateMessagingHoursRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateMessagingHoursCommand(request.Days), ct)).ToMinimalApiResult())
            .WithSummary("Update messaging hours")
            .WithDescription("Sets the clinic's messaging availability hours. Requires Admin role.");

        // GET /api/v1/messaging/stats
        group.MapGet("/stats", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetTriageStatsQuery(), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithSummary("Get messaging triage statistics")
            .WithDescription("Returns triage statistics including conversation counts by status and category. Requires Admin role.");

        // Templates (quick response templates)
        // GET /api/v1/messaging/templates
        group.MapGet("/templates", async (MessageCategory? category, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListTemplatesQuery(category), ct)).ToMinimalApiResult())
            .RequireAuthorization("ClinicStaff")
            .WithName("ListTemplates")
            .WithSummary("List message templates")
            .WithDescription("Returns quick-response templates, optionally filtered by category.");

        // POST /api/v1/messaging/templates
        group.MapPost("/templates", async (CreateTemplateRequest request, IClinicContext clinicContext, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateTemplateCommand(
                clinicContext.ClinicId,
                request.Name,
                request.ContentEn,
                request.ContentAr,
                request.Category), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("CreateTemplate")
            .WithSummary("Create a message template")
            .WithDescription("Creates a new quick-response template with English and Arabic content. Requires Admin role.");

        // PUT /api/v1/messaging/templates/{id}
        group.MapPut("/templates/{id:guid}", async (Guid id, UpdateTemplateRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateTemplateCommand(
                id,
                request.Name,
                request.ContentEn,
                request.ContentAr,
                request.Category), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("UpdateTemplate")
            .WithSummary("Update a message template")
            .WithDescription("Updates an existing quick-response template. Requires Admin role.");

        // DELETE /api/v1/messaging/templates/{id}
        group.MapDelete("/templates/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteTemplateCommand(id), ct)).ToMinimalApiResult())
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithName("DeleteTemplate")
            .WithSummary("Delete a message template")
            .WithDescription("Permanently removes a quick-response template. Requires Admin role.");

        return app;
    }
}
