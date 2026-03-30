using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.SetLineage;

internal record SetLineageCommand(
    Guid ClinicId,
    Guid PatientId,
    Guid? MotherPatientId,
    Guid? FatherPatientId,
    string? RegistryNumber,
    RegistryType? RegistryType) : IRequest<Result<PatientLineageDto>>;
