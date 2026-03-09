using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
using Vetolib.MedicalRecords.Application.Commands.ImportPatients;
using Vetolib.MedicalRecords.Application.Commands.UpdatePatient;
using Vetolib.MedicalRecords.Application.Queries.GetPatientById;
using Vetolib.MedicalRecords.Application.Queries.GetPatientDetail;
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
            .WithName("CreatePatient");

        group.MapGet("/", ListPatients)
            .WithName("ListPatients")
            .CacheOutput("Moderate2min");

        group.MapGet("/{id:guid}", GetPatientById)
            .WithName("GetPatientById")
            .CacheOutput("Moderate2min");

        group.MapGet("/{id:guid}/detail", GetPatientDetail)
            .WithName("GetPatientDetail")
            .CacheOutput("Moderate2min");

        group.MapPatch("/{id:guid}", UpdatePatient)
            .RequireAuthorization("VetOrAdmin")
            .WithName("UpdatePatient");

        group.MapPost("/import", ImportPatients)
            .RequireAuthorization("VetOrAdmin")
            .WithName("ImportPatients")
            .DisableAntiforgery();

        group.MapGet("/import/template", GetImportTemplate)
            .RequireAuthorization()
            .WithName("GetImportTemplate");

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
            request.OwnerPhone);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ListPatients(
        ISender sender,
        string? name = null,
        Species? species = null,
        int page = 1,
        int pageSize = 20)
    {
        return (await sender.Send(new ListPatientsQuery(name, species, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize))).ToMinimalApiResult();
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
            request.OwnerPhone);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> ImportPatients(
        IFormFile file,
        IClinicContext clinicContext,
        ISender sender)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest("CSV file is required.");

        await using var stream = file.OpenReadStream();
        var cmd = new ImportPatientsCommand(clinicContext.ClinicId, stream);
        return (await sender.Send(cmd)).ToMinimalApiResult();
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
}
