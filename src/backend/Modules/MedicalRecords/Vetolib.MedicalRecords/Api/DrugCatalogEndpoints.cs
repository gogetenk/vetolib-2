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
            .WithName("SearchDrugCatalog")
            .WithSummary("Search the drug catalog")
            .WithDescription("Returns drugs matching an optional search query. Limited to a configurable number of results.");

        group.MapGet("/{id:guid}", GetDrugById)
            .WithName("GetDrugCatalogEntryById")
            .WithSummary("Get a drug catalog entry by ID")
            .WithDescription("Returns full details of a specific drug catalog entry including INN name, display name, and category.");

        group.MapPost("/", AddCustomDrug)
            .WithName("AddCustomDrugCatalogEntry")
            .WithSummary("Add a custom drug to the catalog")
            .WithDescription("Creates a clinic-specific drug entry in the catalog. Requires Vet or Admin role.")
            .RequireAuthorization("VetOrAdmin");

        var prescriptionsGroup = app.MapGroup("/api/v1/medical-records/prescriptions")
            .RequireAuthorization()
            .WithTags("Prescriptions");

        prescriptionsGroup.MapPost("/preflight", PrescriptionPreflight)
            .WithName("PrescriptionPreflight")
            .WithSummary("Prescription preflight check")
            .WithDescription("Validates a prescription before submission, checking dosage ranges and patient weight compatibility.");

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
