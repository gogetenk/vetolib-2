using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Messaging.Application.Commands.SendWhatsAppTest;
using Vetolib.Messaging.Application.Commands.UpdateWhatsAppConfig;
using Vetolib.Messaging.Application.Queries.GetWhatsAppConfig;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Api;

internal static class WhatsAppEndpoints
{
    private const string AdminRole = "Admin";

    internal static IEndpointRouteBuilder MapWhatsAppEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/messaging/whatsapp")
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .WithTags("WhatsApp");

        // GET /api/v1/messaging/whatsapp/config
        group.MapGet("/config", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetWhatsAppConfigQuery(), ct)).ToMinimalApiResult()
        ).WithName("GetWhatsAppConfig")
          .WithSummary("Get WhatsApp configuration")
          .WithDescription("Returns the current WhatsApp Business API configuration for the clinic. Requires Admin role.");

        // PUT /api/v1/messaging/whatsapp/config
        group.MapPut("/config", async (
            WhatsAppConfigRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new UpdateWhatsAppConfigCommand(
                request.WabaId,
                request.PhoneNumberId,
                request.AccessToken);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("UpdateWhatsAppConfig")
          .WithSummary("Update WhatsApp configuration")
          .WithDescription("Sets the WhatsApp Business API credentials (WABA ID, phone number ID, access token). Requires Admin role.");

        // POST /api/v1/messaging/whatsapp/test
        group.MapPost("/test", async (
            WhatsAppTestRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var cmd = new SendWhatsAppTestCommand(
                request.RecipientPhone,
                request.TemplateName);
            return (await sender.Send(cmd, ct)).ToMinimalApiResult();
        }).WithName("SendWhatsAppTest")
          .WithSummary("Send a WhatsApp test message")
          .WithDescription("Sends a test WhatsApp message to verify the integration is configured correctly. Requires Admin role.");

        return app;
    }
}
