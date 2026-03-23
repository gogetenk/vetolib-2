using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.AI.Application.Commands.GenerateHealthAlerts;

namespace Vetolib.AI.Api;

internal static class HealthAlertEndpoints
{
    internal static IEndpointRouteBuilder MapHealthAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/ai/health-alerts")
            .RequireAuthorization("VetOrAdmin")
            .RequireRateLimiting("api")
            .WithTags("HealthAlerts");

        group.MapPost("/generate", GenerateAlerts)
            .WithName("GenerateHealthAlerts");

        return app;
    }

    private static async Task<IResult> GenerateAlerts(ISender sender)
    {
        return (await sender.Send(new GenerateHealthAlertsCommand())).ToMinimalApiResult();
    }
}
