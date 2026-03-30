using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.Billing.Application.Commands.SubmitEReporting;
using Vetolib.Billing.Application.Queries.GetEReportingPeriods;
using Vetolib.Billing.Contracts;

namespace Vetolib.Billing.Api;

internal static class EReportingEndpoints
{
    internal static IEndpointRouteBuilder MapEReportingApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/billing/ereporting")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("EReporting");

        group.MapPost("/submit", SubmitEReporting)
            .WithName("SubmitEReporting")
            .WithSummary("Submit e-reporting data")
            .WithDescription("Submits aggregated transaction data for a period to the tax authority. Requires Admin role.");

        group.MapGet("/periods", GetEReportingPeriods)
            .WithName("GetEReportingPeriods")
            .WithSummary("List e-reporting periods")
            .WithDescription("Returns available e-reporting periods with their submission status. Requires Admin role.");

        return app;
    }

    private static async Task<IResult> SubmitEReporting(
        SubmitEReportingRequest request,
        ISender sender)
        => (await sender.Send(new SubmitEReportingCommand(request.PeriodStart, request.PeriodEnd)))
            .ToMinimalApiResult();

    private static async Task<IResult> GetEReportingPeriods(
        ISender sender)
        => (await sender.Send(new GetEReportingPeriodsQuery()))
            .ToMinimalApiResult();
}
