using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.AI.Application.Commands.AcknowledgeHealthAlert;
using Vetolib.AI.Application.Commands.ConvertAlertToAppointment;
using Vetolib.AI.Application.Commands.DismissHealthAlert;
using Vetolib.AI.Application.Commands.GenerateHealthAlerts;
using Vetolib.AI.Application.Queries.GetHealthAlerts;
using Vetolib.AI.Application.Queries.GetPatientHealthAlerts;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Api;

internal static class HealthAlertEndpoints
{
    internal static IEndpointRouteBuilder MapHealthAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/ai/health-alerts")
            .RequireAuthorization("ClinicStaff")
            .RequireRateLimiting("api")
            .WithTags("AI - Health Alerts");

        group.MapGet("/", GetHealthAlerts)
            .WithName("GetHealthAlerts")
            .WithSummary("List health alerts")
            .WithDescription("Returns health alerts for the clinic, optionally filtered by severity, status, or patient.");

        group.MapGet("/patient/{patientId:guid}", GetPatientHealthAlerts)
            .WithName("GetPatientHealthAlerts")
            .WithSummary("Get health alerts for a patient")
            .WithDescription("Returns all health alerts for a specific patient.");

        group.MapPost("/generate", GenerateAlerts)
            .WithName("GenerateHealthAlerts")
            .RequireAuthorization("VetOrAdmin")
            .WithSummary("Generate health alerts")
            .WithDescription("Triggers AI analysis to generate proactive health alerts based on patient data. Requires Vet or Admin role.");

        group.MapPatch("/{id:guid}/dismiss", DismissHealthAlert)
            .WithName("DismissHealthAlert")
            .RequireAuthorization("VetOrAdmin")
            .WithSummary("Dismiss a health alert")
            .WithDescription("Dismisses a health alert with a reason. Requires Vet or Admin role.");

        group.MapPatch("/{id:guid}/acknowledge", AcknowledgeHealthAlert)
            .WithName("AcknowledgeHealthAlert")
            .WithSummary("Acknowledge a health alert")
            .WithDescription("Marks a health alert as acknowledged by the staff member.");

        group.MapPost("/{id:guid}/convert-to-appointment", ConvertAlertToAppointment)
            .WithName("ConvertAlertToAppointment")
            .RequireAuthorization("VetOrAdmin")
            .WithSummary("Convert alert to appointment")
            .WithDescription("Creates an appointment from a health alert to address the identified concern. Requires Vet or Admin role.");

        return app;
    }

    private static async Task<IResult> GetHealthAlerts(
        ISender sender,
        HealthAlertSeverity? severity = null,
        HealthAlertStatus? status = null,
        Guid? patientId = null)
    {
        var query = new GetHealthAlertsQuery(severity, status, patientId);
        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetPatientHealthAlerts(
        Guid patientId,
        ISender sender)
    {
        var query = new GetPatientHealthAlertsQuery(patientId);
        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static async Task<IResult> GenerateAlerts(ISender sender)
    {
        return (await sender.Send(new GenerateHealthAlertsCommand())).ToMinimalApiResult();
    }

    private static async Task<IResult> DismissHealthAlert(
        Guid id,
        DismissRequest request,
        ClaimsPrincipal user,
        ISender sender)
    {
        var vetName = user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? "Unknown";

        var cmd = new DismissHealthAlertCommand(id, request.Reason, vetName);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> AcknowledgeHealthAlert(
        Guid id,
        ISender sender)
    {
        var cmd = new AcknowledgeHealthAlertCommand(id);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ConvertAlertToAppointment(
        Guid id,
        ISender sender)
    {
        var cmd = new ConvertAlertToAppointmentCommand(id);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}

internal record DismissRequest(string Reason);
