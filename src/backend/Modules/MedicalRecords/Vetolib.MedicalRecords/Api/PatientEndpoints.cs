using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
using Vetolib.MedicalRecords.Application.Commands.DeletePatientPhoto;
using Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;
using Vetolib.MedicalRecords.Application.Commands.ImportPatients;
using Vetolib.MedicalRecords.Application.Commands.TransferPatient;
using Vetolib.MedicalRecords.Application.Commands.UpdatePatient;
using Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;
using Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;
using Vetolib.MedicalRecords.Application.Services;
using Vetolib.MedicalRecords.Application.Queries.GetPatientById;
using Vetolib.MedicalRecords.Application.Queries.GetPatientDetail;
using Vetolib.MedicalRecords.Application.Queries.GetPatientPhoto;
using Vetolib.MedicalRecords.Application.Queries.GetPatientSummary;
using Vetolib.MedicalRecords.Application.Queries.ListPatients;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class PatientEndpoints
{
    internal static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Patients");

        group.MapPost("/", CreatePatient)
            .RequireAuthorization("VetOrAdmin")
            .WithName("CreatePatient")
            .WithSummary("Register a new patient")
            .WithDescription("Creates a new patient (animal) record with species, breed, owner information, and optional microchip number.");

        group.MapGet("/", ListPatients)
            .WithName("ListPatients")
            .WithSummary("Search and list patients with advanced filters")
            .WithDescription("Returns a paginated, filterable list of patients. Supports filtering by partial name (case-insensitive), species, microchip number, and owner phone number.")
            .CacheOutput(p => p.SetVaryByQuery("page", "pageSize", "species", "name", "microchip", "ownerPhone").SetVaryByHeader("Authorization").Expire(TimeSpan.FromSeconds(30)).Tag("patients"));

        group.MapGet("/{id:guid}", GetPatientById)
            .WithName("GetPatientById")
            .WithSummary("Get patient by ID")
            .WithDescription("Returns the basic details of a single patient record.")
            .CacheOutput("Moderate2min");

        group.MapGet("/{id:guid}/detail", GetPatientDetail)
            .WithName("GetPatientDetail")
            .WithSummary("Get patient extended detail")
            .WithDescription("Returns full patient details including recent medical records, weight history, and breeding information.")
            .CacheOutput("Moderate2min");

        group.MapGet("/{id:guid}/export/summary", GetPatientSummary)
            .WithName("GetPatientSummary")
            .WithSummary("Export patient medical summary")
            .WithDescription("Returns a structured medical summary for a patient including patient info, owner info, recent medical records, active prescriptions, vaccinations, and health alerts. Designed for sharing with other clinics or pet owners.");

        group.MapGet("/{id:guid}/export/fhir", ExportPatientFhir)
            .WithName("ExportPatientFhir")
            .WithSummary("Export patient as FHIR R4 Bundle")
            .WithDescription("Returns a FHIR R4 JSON Bundle containing the patient record mapped to standard FHIR resources: Patient (with patient-animal extension), RelatedPerson (owner), Encounter (medical records), MedicationRequest (prescriptions), and Observation (weight entries). Microchip is mapped to ISO 11784/11785 identifier.")
            .Produces<string>(200, FhirBundleResult.FhirJsonContentType);

        group.MapPatch("/{id:guid}", UpdatePatient)
            .RequireAuthorization("VetOrAdmin")
            .WithName("UpdatePatient")
            .WithSummary("Update a patient")
            .WithDescription("Updates patient information such as name, breed, owner details, or microchip number.");

        group.MapPost("/import", ImportPatients)
            .RequireAuthorization("VetOrAdmin")
            .WithName("ImportPatients")
            .WithSummary("Import patients from CSV")
            .WithDescription("Bulk-imports patient records from a CSV file. Use GET /import/template to download the expected format.")
            .DisableAntiforgery();

        group.MapPost("/import/fhir", ImportPatientFhir)
            .RequireAuthorization("VetOrAdmin")
            .WithName("ImportPatientFhir")
            .WithSummary("Import patient from FHIR R4 Bundle")
            .WithDescription("Imports a patient and associated medical records from a FHIR R4 JSON Bundle. Parses Patient, Encounter, MedicationRequest, and Observation (body-weight) resources. Deduplicates by microchip number — if a patient with the same microchip already exists, records are merged into the existing patient.");

        group.MapGet("/import/template", GetImportTemplate)
            .RequireAuthorization()
            .WithName("GetImportTemplate")
            .WithSummary("Download CSV import template")
            .WithDescription("Returns a sample CSV file with the expected columns and format for patient bulk import.");

        group.MapPost("/{id:guid}/photo", UploadPatientPhoto)
            .RequireAuthorization("VetOrAdmin")
            .WithName("UploadPatientPhoto")
            .WithSummary("Upload patient photo")
            .WithDescription("Uploads a photo for a patient. Accepts multipart/form-data with a single image file (JPEG, PNG, or WebP). Max 5 MB.")
            .DisableAntiforgery();

        group.MapGet("/{id:guid}/photo", GetPatientPhoto)
            .WithName("GetPatientPhoto")
            .WithSummary("Get patient photo")
            .WithDescription("Returns the patient's photo as a binary image response.");

        group.MapDelete("/{id:guid}/photo", DeletePatientPhoto)
            .RequireAuthorization("VetOrAdmin")
            .WithName("DeletePatientPhoto")
            .WithSummary("Delete patient photo")
            .WithDescription("Removes the photo from a patient profile.");

        group.MapPost("/{id:guid}/transfer", TransferPatientEndpoint)
            .RequireAuthorization("VetOrAdmin")
            .WithName("TransferPatient")
            .WithSummary("Transfer patient to another clinic")
            .WithDescription("Transfers a patient to a target clinic. Creates a full copy of the patient (and optionally medical records and weight history) in the target clinic, and marks the source patient as transferred. The source clinic keeps a read-only copy.");

        return app;
    }

    private static async Task<IResult> CreatePatient(
        CreatePatientRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new CreatePatientCommand(
            clinicContext.ClinicId,
            request.Name,
            request.Species,
            request.Breed,
            request.BirthDate,
            request.OwnerName,
            request.OwnerPhone,
            request.Sex,
            request.MicrochipNumber,
            request.OwnerEmail);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListPatients(
        ISender sender,
        string? name = null,
        Species? species = null,
        string? microchip = null,
        string? ownerPhone = null,
        int page = 1,
        int pageSize = 20)
    {
        if (pageSize is < 1 or > 200) pageSize = 20;
        return (await sender.Send(new ListPatientsQuery(name, species, microchip, ownerPhone, page <= 0 ? 1 : page, pageSize))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetPatientById(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetPatientByIdQuery(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> GetPatientDetail(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetPatientDetailQuery(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> UpdatePatient(
        Guid id,
        UpdatePatientRequest request,
        ISender sender)
    {
        var cmd = new UpdatePatientCommand(
            id,
            request.Name,
            request.Species,
            request.Breed,
            request.BirthDate,
            request.OwnerName,
            request.OwnerPhone,
            request.Sex,
            request.MicrochipNumber);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ImportPatients(
        IFormFile file,
        IClinicContext clinicContext,
        ISender sender)
    {
        const long maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        if (file is null || file.Length == 0)
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError("CSV file is required.")).ToMinimalApiResult();

        if (file.Length > maxFileSizeBytes)
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError("CSV file exceeds maximum allowed size of 5 MB.")).ToMinimalApiResult();

        await using var stream = file.OpenReadStream();
        var cmd = new ImportPatientsCommand(clinicContext.ClinicId, stream);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetPatientSummary(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetPatientSummaryQuery(id))).ToMinimalApiResult();
    }

    private static IResult GetImportTemplate()
    {
        const string template =
            "PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone\r\n" +
            "Max,Dog,Golden Retriever,2021-05-10,Ahmed Al-Rashid,ahmed@email.ae,+971 50 123 4567\r\n" +
            "Luna,Cat,Siamese,2020-03-22,Fatima Hassan,fatima@email.ae,+971 55 987 6543\r\n";

        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        return Results.File(bytes, "text/csv", "patients_import_template.csv");
    }

    private static async Task<IResult> UploadPatientPhoto(
        Guid id,
        IFormFile file,
        ISender sender)
    {
        const long maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        if (file is null || file.Length == 0)
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError("Photo file is required.")).ToMinimalApiResult();

        if (file.Length > maxFileSizeBytes)
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError("Photo exceeds maximum allowed size of 5 MB.")).ToMinimalApiResult();

        // Content-type allowlist check
        if (!PhotoFileValidator.AllowedContentTypes.Contains(file.ContentType))
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError(
                "Invalid content type. Allowed types: image/jpeg, image/png, image/webp.")).ToMinimalApiResult();

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var photoData = memoryStream.ToArray();

        // Magic-byte validation: actual file content must match declared content type
        if (!PhotoFileValidator.IsValid(file.ContentType, photoData))
            return Ardalis.Result.Result.Invalid(new Ardalis.Result.ValidationError(
                "File content does not match declared content type. The file may be corrupted or disguised.")).ToMinimalApiResult();

        var cmd = new UploadPatientPhotoCommand(id, photoData, file.ContentType);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> GetPatientPhoto(
        Guid id,
        ISender sender)
    {
        var result = await sender.Send(new GetPatientPhotoQuery(id));

        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        return Results.File(result.Value.Data, result.Value.ContentType);
    }

    private static async Task<IResult> DeletePatientPhoto(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new DeletePatientPhotoCommand(id))).ToMinimalApiResult();
    }

    private static async Task<IResult> ExportPatientFhir(
        Guid id,
        ISender sender)
    {
        var result = await sender.Send(new ExportPatientFhirQuery(id));

        if (!result.IsSuccess)
            return result.ToMinimalApiResult();

        return Results.Content(result.Value.Json, result.Value.ContentType);
    }

    private static async Task<IResult> ImportPatientFhir(
        FhirImportRequest request,
        IClinicContext clinicContext,
        ISender sender)
    {
        var cmd = new ImportPatientFhirCommand(clinicContext.ClinicId, request.FhirBundleJson);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> TransferPatientEndpoint(
        Guid id,
        TransferPatientRequest request,
        IClinicContext clinicContext,
        ClaimsPrincipal user,
        ISender sender)
    {
        var transferredBy = user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? "Unknown";

        var cmd = new TransferPatientCommand(
            clinicContext.ClinicId,
            id,
            request.TargetClinicId,
            request.IncludeRecords,
            request.IncludeWeightHistory,
            transferredBy);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}

internal record FhirImportRequest(string FhirBundleJson);
