using System.Security.Claims;
using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
using Vetolib.MedicalRecords.Application.Queries.GetPatientById;
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

        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> CreatePatient(
        CreatePatientRequest request,
        ClaimsPrincipal user,
        IClinicContext clinicContext,
        ISender sender)
    {
        // Only VET and Admin can create patients
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        if (role is not ("Vet" or "Admin"))
            return Ardalis.Result.Result<PatientDto>.Forbidden().ToMinimalApiResult();

        var cmd = new CreatePatientCommand(
            clinicContext.ClinicId,
            request.Name,
            request.Species,
            request.Breed,
            request.DateOfBirth,
            request.OwnerId);

        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ListPatients(
        ISender sender)
    {
        return (await sender.Send(new ListPatientsQuery())).ToMinimalApiResult();
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> GetPatientById(
        Guid id,
        ISender sender)
    {
        return (await sender.Send(new GetPatientByIdQuery(id))).ToMinimalApiResult();
    }
}
