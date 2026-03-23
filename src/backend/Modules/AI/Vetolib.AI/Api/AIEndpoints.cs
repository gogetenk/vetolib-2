using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.AI.Application.Commands.AcceptTriage;
using Vetolib.AI.Application.Commands.GenerateSoapNotes;
using Vetolib.AI.Application.Commands.OverrideTriage;
using Vetolib.AI.Application.Commands.PredictNoShow;
using Vetolib.AI.Application.Commands.PredictNoShowBatch;
using Vetolib.AI.Application.Commands.TriageSymptoms;
using Vetolib.AI.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.AI.Api;

internal static class AIEndpoints
{
    internal static IEndpointRouteBuilder MapAIApiEndpoints(this IEndpointRouteBuilder app)
    {
        // Health Alert endpoints are in a separate file for clarity
        app.MapHealthAlertEndpoints();

        var group = app.MapGroup("/api/v1/ai")
            .RequireAuthorization("ClinicStaff")
            .RequireRateLimiting("api")
            .WithTags("AI");

        group.MapPost("/triage", TriageSymptoms)
            .WithName("TriageSymptoms");

        group.MapPut("/triage/{id:guid}/accept", AcceptTriage)
            .WithName("AcceptTriage")
            .RequireAuthorization("VetOrAdmin");

        group.MapPut("/triage/{id:guid}/override", OverrideTriage)
            .WithName("OverrideTriage")
            .RequireAuthorization("VetOrAdmin");

        // No-show prediction — visible only to clinic staff (never to Owner role)
        // The group already requires "ClinicStaff" which excludes Owner role.
        group.MapGet("/no-show-prediction/{appointmentId:guid}", PredictNoShow)
            .WithName("PredictNoShow");

        group.MapPost("/no-show-predictions/batch", PredictNoShowBatch)
            .WithName("PredictNoShowBatch");

        // SOAP notes generation — AI-assisted medical record writing
        group.MapPost("/soap-notes", GenerateSoapNotes)
            .WithName("GenerateSoapNotes")
            .RequireAuthorization("VetOrAdmin");

        // Drug interaction checking — used by prescription form before saving
        group.MapPost("/check-interactions", CheckInteractions)
            .WithName("CheckInteractions")
            .RequireAuthorization("VetOrAdmin");

        return app;
    }

    private static async Task<IResult> TriageSymptoms(
        TriageRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var createdBy = user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "unknown";

        Guid.TryParse(createdBy, out var userId);

        var cmd = new TriageSymptomsCommand(
            ClinicId: clinicContext.ClinicId,
            UserId: userId,
            Symptoms: request.Symptoms,
            Species: request.Species,
            Breed: request.Breed,
            AgeMonths: request.AgeMonths,
            WeightKg: request.WeightKg,
            CreatedBy: createdBy);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> AcceptTriage(
        Guid id,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new AcceptTriageCommand(id, clinicContext.ClinicId);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> OverrideTriage(
        Guid id,
        OverrideRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new OverrideTriageCommand(id, request.NewSeverity, clinicContext.ClinicId);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> PredictNoShow(
        Guid appointmentId,
        ClaimsPrincipal user,
        ISender sender)
    {
        var sub = user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(sub, out var userId);

        var cmd = new PredictNoShowCommand(appointmentId, userId);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> PredictNoShowBatch(
        NoShowBatchRequest request,
        ISender sender)
    {
        var cmd = new PredictNoShowBatchCommand(request.Date);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> CheckInteractions(
        CheckInteractionsRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var query = new CheckInteractionsQuery(
            request.PatientId,
            request.DrugCatalogEntryId,
            request.DosageAmount,
            clinicContext.ClinicId);

        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static async Task<IResult> GenerateSoapNotes(
        SoapNotesRequest request,
        ISender sender)
    {
        var cmd = new GenerateSoapNotesCommand(
            Species: request.Species,
            Breed: request.Breed,
            PatientName: request.PatientName,
            Symptoms: request.Symptoms,
            Vitals: request.Vitals,
            Diagnosis: request.Diagnosis,
            TreatmentPlan: request.TreatmentPlan,
            Prescriptions: request.Prescriptions);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}

internal record TriageRequest(
    string Symptoms,
    string Species,
    string? Breed,
    int? AgeMonths,
    decimal? WeightKg);

internal record OverrideRequest(AISeverity NewSeverity);

internal record NoShowBatchRequest(DateOnly Date);

internal record CheckInteractionsRequest(
    Guid PatientId,
    Guid DrugCatalogEntryId,
    decimal? DosageAmount);

internal record SoapNotesRequest(
    string Species,
    string Breed,
    string PatientName,
    string Symptoms,
    string Vitals,
    string Diagnosis,
    string TreatmentPlan,
    List<string> Prescriptions);
