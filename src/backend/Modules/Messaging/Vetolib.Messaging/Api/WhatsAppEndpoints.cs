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
        ).WithName("GetWhatsAppConfig");

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
        }).WithName("UpdateWhatsAppConfig");

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
        }).WithName("SendWhatsAppTest");

        return app;
    }
}
