using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class DrugCatalogEndpoints
{
    internal static IEndpointRouteBuilder MapDrugCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/medical-records/drugs")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("DrugCatalog");

        group.MapGet("/", SearchDrugs)
            .WithName("SearchDrugCatalog");

        group.MapGet("/{id:guid}", GetDrugById)
            .WithName("GetDrugCatalogEntryById");

        group.MapPost("/", AddCustomDrug)
            .WithName("AddCustomDrugCatalogEntry")
            .RequireAuthorization("VetOrAdmin");

        var prescriptionsGroup = app.MapGroup("/api/v1/medical-records/prescriptions")
            .RequireAuthorization()
            .WithTags("Prescriptions");

        prescriptionsGroup.MapPost("/preflight", PrescriptionPreflight)
            .WithName("PrescriptionPreflight");

        return app;
    }

    private static async Task<IResult> SearchDrugs(
        ISender sender,
        string? search = null,
        int limit = 20)
    {
        return (await sender.Send(new SearchDrugCatalogQuery(search, limit))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetDrugById(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetDrugCatalogEntryByIdQuery(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> PrescriptionPreflight(
        PreflightRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var query = new GetPrescriptionPreflightQuery(
            PatientId: request.PatientId,
            DrugCatalogEntryId: request.DrugCatalogEntryId,
            DosageAmount: request.DosageAmount,
            ClinicId: clinicContext.ClinicId);

        return (await sender.Send(query)).ToMinimalApiResult();
    }

    private static async Task<IResult> AddCustomDrug(
        AddCustomDrugRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var command = new AddCustomDrugCommand(
            InnName: request.InnName,
            DisplayName: request.DisplayName,
            Category: request.Category,
            ClinicId: clinicContext.ClinicId);

        return (await sender.Send(command)).ToMinimalApiResult();
    }

    private record PreflightRequest(
        Guid PatientId,
        Guid DrugCatalogEntryId,
        decimal? DosageAmount);
}
