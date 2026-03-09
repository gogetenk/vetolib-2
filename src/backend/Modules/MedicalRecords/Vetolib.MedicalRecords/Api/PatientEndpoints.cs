using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
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
            .WithTags("Patients");

        group.MapPost("/", CreatePatient)
            .WithName("CreatePatient");

        group.MapGet("/", ListPatients)
            .WithName("ListPatients");

        group.MapGet("/{id:guid}", GetPatientById)
            .WithName("GetPatientById");

        group.MapGet("/{id:guid}/detail", GetPatientDetail)
            .WithName("GetPatientDetail");

        group.MapPatch("/{id:guid}", UpdatePatient)
            .WithName("UpdatePatient");

        return app;
    }

    private static async Task<IResult> CreatePatient(
        CreatePatientRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin"))
            return Ardalis.Result.Result<PatientDto>.Forbidden().ToMinimalApiResult();

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
        string? name,
        Species? species,
        int page,
        int pageSize,
        ISender sender)
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
        ClaimsPrincipal user,
        ISender sender)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin"))
            return Ardalis.Result.Result<PatientDto>.Forbidden().ToMinimalApiResult();

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
}
