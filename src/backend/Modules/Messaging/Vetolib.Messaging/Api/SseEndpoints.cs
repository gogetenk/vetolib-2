using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Application.Services.SSE;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Api;

internal static class SseEndpoints
{
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(30);

    internal static IEndpointRouteBuilder MapMessagingSseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/messaging/sse", HandleSseAsync)
            .RequireAuthorization()
            .WithTags("Messaging")
            .WithName("MessagingSSE")
            .WithSummary("Messaging real-time event stream")
            .WithDescription("Server-Sent Events (SSE) endpoint for real-time messaging notifications. Streams new messages, status changes, and unread counts.")
            // Per-IP sliding window: max 5 SSE connection attempts per minute
            .RequireRateLimiting("sse");

        return app;
    }

    private static async Task HandleSseAsync(
        HttpContext context,
        IMessagingEventBroadcaster broadcaster,
        IClinicContext clinicContext,
        MessagingDbContext db,
        CancellationToken ct)
    {
        var user = context.User;
        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var clinicId = clinicContext.ClinicId;
        var connectionId = Guid.NewGuid().ToString("N");

        // Set SSE headers before writing anything
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Connection = "keep-alive";
        context.Response.Headers["X-Accel-Buffering"] = "no";

        var subscribeResult = broadcaster.Subscribe(connectionId, clinicId, role);
        if (!subscribeResult.IsSuccess)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "30";
            await context.Response.WriteAsync(
                subscribeResult.Errors.FirstOrDefault() ?? "Too many SSE connections.", ct);
            return;
        }

        var reader = subscribeResult.Value;

        try
        {
            // On connect: send current unread count immediately
            var unreadCount = await GetUnreadCountAsync(db, role, ct);
            await WriteEventAsync(context.Response, "unread-count", new
            {
                type = "unread-count",
                unreadCount
            }, ct);

            await context.Response.Body.FlushAsync(ct);

            using var keepAliveTimer = new PeriodicTimer(KeepAliveInterval);
            var keepAliveTask = RunKeepAliveAsync(context.Response, keepAliveTimer, ct);
            var readTask = ReadEventsAsync(context.Response, reader, ct);

            await Task.WhenAny(keepAliveTask, readTask);

            // Propagate cancellation / exceptions from whichever completed first
            // If CT is cancelled (client disconnected), both tasks will terminate naturally.
        }
        finally
        {
            broadcaster.Unsubscribe(connectionId);
        }
    }

    private static async Task ReadEventsAsync(
        HttpResponse response,
        System.Threading.Channels.ChannelReader<MessagingEvent> reader,
        CancellationToken ct)
    {
        await foreach (var evt in reader.ReadAllAsync(ct))
        {
            var eventName = evt.Type;
            object payload = evt.Type switch
            {
                "new-message" => new
                {
                    type = evt.Type,
                    conversationId = evt.ConversationId,
                    category = evt.Category?.ToString(),
                    preview = evt.Preview
                },
                "conversation-updated" => new
                {
                    type = evt.Type,
                    conversationId = evt.ConversationId,
                    newStatus = evt.NewStatus?.ToString()
                },
                "unread-count" => new
                {
                    type = evt.Type,
                    unreadCount = evt.UnreadCount
                },
                _ => new { type = evt.Type }
            };

            await WriteEventAsync(response, eventName, payload, ct);
            await response.Body.FlushAsync(ct);
        }
    }

    private static async Task RunKeepAliveAsync(
        HttpResponse response,
        PeriodicTimer timer,
        CancellationToken ct)
    {
        while (await timer.WaitForNextTickAsync(ct))
        {
            // SSE comment — keeps the connection alive through proxies
            var comment = Encoding.UTF8.GetBytes(": keep-alive\n\n");
            await response.Body.WriteAsync(comment, ct);
            await response.Body.FlushAsync(ct);
        }
    }

    private static async Task WriteEventAsync(
        HttpResponse response,
        string eventName,
        object payload,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var sb = new StringBuilder();
        sb.Append("event: ").AppendLine(eventName);
        sb.Append("data: ").AppendLine(json);
        sb.AppendLine();

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        await response.Body.WriteAsync(bytes, ct);
    }

    private static async Task<int> GetUnreadCountAsync(
        MessagingDbContext db,
        string role,
        CancellationToken ct)
    {
        // "Unread" = Open conversations (not yet replied to by staff)
        // Receptionist does not see medical categories
        var medicalCategories = new[]
        {
            MessageCategory.MedicalUrgency,
            MessageCategory.PostOperativeFollowUp,
            MessageCategory.MedicalQuestion
        };

        var query = db.Conversations
            .Where(c => c.Status == ConversationStatus.Open && !c.IsSpam);

        if (IsReceptionist(role))
            query = query.Where(c => !medicalCategories.Contains(c.Category));

        return await query.CountAsync(ct);
    }

    private static bool IsReceptionist(string role) =>
        string.Equals(role, "Receptionist", StringComparison.OrdinalIgnoreCase);
}
