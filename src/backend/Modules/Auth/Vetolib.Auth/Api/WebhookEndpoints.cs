using System.Text.Json;
using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Auth.Application.Commands.DeactivateWebhook;
using Vetolib.Auth.Application.Commands.ReceiveWebhook;
using Vetolib.Auth.Application.Commands.RegisterWebhook;
using Vetolib.Auth.Application.Queries.ListWebhooks;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Api;

internal static class WebhookEndpoints
{
    internal static IEndpointRouteBuilder MapWebhookApiEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/v1/webhooks")
            .WithTags("Webhooks")
            .RequireAuthorization("VetOrAdmin");

        adminGroup.MapPost("/", RegisterWebhook)
            .WithName("RegisterWebhook")
            .WithSummary("Register a webhook")
            .WithDescription("Registers a new webhook for the current clinic. Requires Admin role.");

        adminGroup.MapGet("/", ListWebhooks)
            .WithName("ListWebhooks")
            .WithSummary("List webhook registrations")
            .WithDescription("Lists all webhook registrations for the current clinic.");

        adminGroup.MapDelete("/{id:guid}", DeactivateWebhook)
            .WithName("DeactivateWebhook")
            .WithSummary("Deactivate a webhook")
            .WithDescription("Deactivates a webhook registration by ID.");

        // Public endpoint — no auth, HMAC-verified
        var publicGroup = app.MapGroup("/api/v1/webhooks")
            .WithTags("Webhooks");

        publicGroup.MapPost("/receive", ReceiveWebhook)
            .WithName("ReceiveWebhook")
            .AllowAnonymous()
            .WithSummary("Receive a webhook")
            .WithDescription("Public endpoint for external services to push data. Verified via HMAC-SHA256 signature.");

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RegisterWebhook(
        RegisterWebhookRequest request,
        ISender sender)
        => (await sender.Send(new RegisterWebhookCommand(request.Name, request.Secret, request.EventTypes)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListWebhooks(
        ISender sender)
        => (await sender.Send(new ListWebhooksQuery()))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> DeactivateWebhook(
        Guid id,
        ISender sender)
        => (await sender.Send(new DeactivateWebhookCommand(id)))
            .ToMinimalApiResult();

    private static async Task<Microsoft.AspNetCore.Http.IResult> ReceiveWebhook(
        HttpRequest httpRequest,
        ISender sender)
    {
        var signature = httpRequest.Headers["X-Webhook-Signature"].FirstOrDefault() ?? string.Empty;

        // Read raw body for HMAC verification
        httpRequest.EnableBuffering();
        using var reader = new StreamReader(httpRequest.Body);
        var rawBody = await reader.ReadToEndAsync();

        // Parse the body
        ReceiveWebhookRequest? body;
        try
        {
            body = JsonSerializer.Deserialize<ReceiveWebhookRequest>(rawBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return Result.Invalid(new ValidationError("Body", "Invalid JSON body")).ToMinimalApiResult();
        }

        if (body is null)
            return Result.Invalid(new ValidationError("Body", "Request body is required")).ToMinimalApiResult();

        return (await sender.Send(new ReceiveWebhookCommand(signature, rawBody, body.EventType, body.Payload)))
            .ToMinimalApiResult();
    }
}
