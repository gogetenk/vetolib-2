using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;
using Vetolib.MedicalRecords.Application.Commands.AddPrescription;
using Vetolib.MedicalRecords.Application.Queries.ListMedicalRecords;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Api;

internal static class MedicalRecordEndpoints
{
    internal static IEndpointRouteBuilder MapMedicalRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients/{patientId:guid}/records")
            .RequireAuthorization()
            .WithTags("MedicalRecords");

        group.MapPost("/", AddMedicalRecord)
            .WithName("AddMedicalRecord");

        group.MapGet("/", ListMedicalRecords)
            .WithName("ListMedicalRecords");

        group.MapDelete("/{recordId:guid}", DeleteMedicalRecord)
            .WithName("DeleteMedicalRecord");

        group.MapPost("/{recordId:guid}/prescriptions", AddPrescription)
            .WithName("AddPrescription");

        return app;
    }

    private static async Task<IResult> AddMedicalRecord(
        Guid patientId,
        AddMedicalRecordRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin"))
            return Ardalis.Result.Result<MedicalRecordDto>.Forbidden().ToMinimalApiResult();

        var vetName = user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value
            ?? "Unknown";

        var cmd = new AddMedicalRecordCommand(
            clinicContext.ClinicId,
            patientId,
            request.Diagnosis,
            request.Treatment,
            vetName);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListMedicalRecords(
        Guid patientId,
        ISender sender)
    {
        return (await sender.Send(new ListMedicalRecordsQuery(patientId))).ToMinimalApiResult();
    }

    private static IResult DeleteMedicalRecord(
        Guid patientId,
        Guid recordId)
    {
        // Medical records are immutable — deletion is forbidden
        var result = Ardalis.Result.Result.Error("MEDICAL_RECORD_IMMUTABLE:Un dossier médical ne peut jamais être supprimé");
        return result.ToMinimalApiResult();
    }

    private static async Task<IResult> AddPrescription(
        Guid patientId,
        Guid recordId,
        AddPrescriptionRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin"))
            return Ardalis.Result.Result<PrescriptionDto>.Forbidden().ToMinimalApiResult();

        var vetLicense = user.FindFirst("vetLicense")?.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(vetLicense))
            return Ardalis.Result.Result<PrescriptionDto>.Error("VET_LICENSE_REQUIRED:Numéro de licence vétérinaire requis").ToMinimalApiResult();

        var userIdClaim = user.FindFirst("sub")?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdClaim, out var vetId);

        var cmd = new AddPrescriptionCommand(
            clinicContext.ClinicId,
            recordId,
            patientId,
            request.Medication,
            request.Dosage,
            vetLicense,
            vetId,
            role ?? "Vet",
            request.DrugCatalogEntryId,
            request.DosageAmount,
            request.OverrideJustification);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }
}
