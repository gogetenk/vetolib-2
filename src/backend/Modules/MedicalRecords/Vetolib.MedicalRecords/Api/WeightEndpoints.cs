using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.AddWeightEntry;
using Vetolib.MedicalRecords.Application.Queries.GetWeightCurve;
using Vetolib.MedicalRecords.Application.Queries.GetWeightHistory;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class WeightEndpoints
{
    internal static IEndpointRouteBuilder MapWeightEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients/{patientId:guid}/weights")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Weights");

        group.MapPost("/", AddWeightEntry)
            .RequireAuthorization("VetOrAdmin")
            .WithName("AddWeightEntry")
            .WithSummary("Record a weight entry")
            .WithDescription("Adds a new weight measurement for a patient in kilograms with an optional note. Requires Vet or Admin role.");

        group.MapGet("/", GetWeightHistory)
            .WithName("GetWeightHistory")
            .WithSummary("Get weight history")
            .WithDescription("Returns a paginated list of weight entries for a patient, ordered by most recent first.");

        group.MapGet("/curve", GetWeightCurve)
            .WithName("GetWeightCurve")
            .WithSummary("Get weight curve data")
            .WithDescription("Returns weight data points optimized for chart rendering, including dates and weights in kilograms.");

        return app;
    }

    private static async Task<IResult> AddWeightEntry(
        Guid patientId,
        AddWeightEntryRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var vetName = user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? "Unknown";

        var cmd = new AddWeightEntryCommand(
            clinicContext.ClinicId,
            patientId,
            request.WeightKg,
            vetName,
            request.Note);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetWeightHistory(
        Guid patientId,
        ISender sender,
        int page = 1,
        int pageSize = 20)
    {
        return (await sender.Send(new GetWeightHistoryQuery(patientId, page < 1 ? 1 : page, pageSize < 1 ? 20 : pageSize))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetWeightCurve(
        Guid patientId,
        ISender sender)
    {
        return (await sender.Send(new GetWeightCurveQuery(patientId))).ToMinimalApiResult();
    }
}
