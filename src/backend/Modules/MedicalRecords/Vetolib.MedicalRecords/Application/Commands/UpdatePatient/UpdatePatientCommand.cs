using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.UpdatePatient;

internal record UpdatePatientCommand(
    Guid PatientId,
    string? Name,
    Species? Species,
    string? Breed,
    DateOnly? BirthDate,
    string? OwnerName,
    string? OwnerPhone,
    Sex? Sex = null) : IRequest<Result<PatientDto>>;
