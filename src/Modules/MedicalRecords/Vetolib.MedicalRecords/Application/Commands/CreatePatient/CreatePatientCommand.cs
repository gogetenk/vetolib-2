using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.CreatePatient;

internal record CreatePatientCommand(
    Guid ClinicId,
    string Name,
    string Species,
    string Breed,
    DateTime? DateOfBirth,
    Guid OwnerId) : IRequest<Result<PatientDto>>;
