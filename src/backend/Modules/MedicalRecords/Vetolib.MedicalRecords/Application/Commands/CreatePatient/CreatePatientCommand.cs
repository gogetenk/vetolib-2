using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal record CreatePatientCommand(
    Guid ClinicId,
    string Name,
    Species Species,
    string Breed,
    DateOnly BirthDate,
    string OwnerName,
    string OwnerPhone,
    Sex? Sex = null,
    string? MicrochipNumber = null) : IRequest<Result<PatientDto>>;
