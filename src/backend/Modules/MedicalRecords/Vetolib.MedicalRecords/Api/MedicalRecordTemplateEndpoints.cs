using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.CreateMedicalRecordTemplate;
using Vetolib.MedicalRecords.Application.Commands.DeleteMedicalRecordTemplate;
using Vetolib.MedicalRecords.Application.Commands.UpdateMedicalRecordTemplate;
using Vetolib.MedicalRecords.Application.Queries.ListMedicalRecordTemplates;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class MedicalRecordTemplateEndpoints
{
    internal static IEndpointRouteBuilder MapMedicalRecordTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/medical-records/templates")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("MedicalRecordTemplates");

        group.MapGet("/", ListTemplates)
            .WithName("ListMedicalRecordTemplates")
            .WithSummary("List medical record templates")
            .WithDescription("Returns all medical record templates for the current clinic, including system templates. Supports filtering by category and species.");

        group.MapPost("/", CreateTemplate)
            .RequireAuthorization(policy => policy.RequireRole("Vet", "Admin"))
            .WithName("CreateMedicalRecordTemplate")
            .WithSummary("Create a custom template")
            .WithDescription("Creates a custom medical record template for the current clinic. Requires Vet or Admin role.");

        group.MapPut("/{id:guid}", UpdateTemplate)
            .RequireAuthorization(policy => policy.RequireRole("Vet", "Admin"))
            .WithName("UpdateMedicalRecordTemplate")
            .WithSummary("Update a custom template")
            .WithDescription("Updates a custom medical record template. System templates are read-only and cannot be modified. Requires Vet or Admin role.");

        group.MapDelete("/{id:guid}", DeleteTemplate)
            .RequireAuthorization(policy => policy.RequireRole("Vet", "Admin"))
            .WithName("DeleteMedicalRecordTemplate")
            .WithSummary("Delete a custom template")
            .WithDescription("Deletes a custom medical record template. System templates cannot be deleted. Requires Vet or Admin role.");

        return app;
    }

    private static async Task<IResult> ListTemplates(
        ISender sender,
        TemplateCategory? category = null,
        Species? species = null)
    {
        return (await sender.Send(new ListMedicalRecordTemplatesQuery(category, species))).ToMinimalApiResult();
    }

    private static async Task<IResult> CreateTemplate(
        CreateMedicalRecordTemplateRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            clinicContext.ClinicId,
            request.Name,
            request.Category,
            request.DiagnosisTemplate,
            request.TreatmentTemplate,
            request.NotesTemplate,
            request.Species,
            request.SortOrder);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> UpdateTemplate(
        Guid id,
        UpdateMedicalRecordTemplateRequest request,
        ISender sender)
    {
        var cmd = new UpdateMedicalRecordTemplateCommand(
            id,
            request.Name,
            request.Category,
            request.DiagnosisTemplate,
            request.TreatmentTemplate,
            request.NotesTemplate,
            request.Species,
            request.SortOrder);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> DeleteTemplate(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new DeleteMedicalRecordTemplateCommand(id))).ToMinimalApiResult();
    }
}
